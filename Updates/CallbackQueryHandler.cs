using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SwineBot.Actions.Commands.Duel;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Duels;
using SwineBot.Model;
using SwineBot.Senders;
using SwineBot.ViewModels;
using Telegram.Bot.Types;

namespace SwineBot.Updates;

public class CallbackQueryHandler(ILogger<CallbackQueryHandler> logger, UserContext context, IMessageEditor messageEditor, ICallbackQueryAnswerer answerer, IMessageFactory messageFactory, IDateTimeNowProvider dtProvider, DuelStatsProvider statsProvider) : UpdateTypeHandler<CallbackQuery>
{
    public override async Task<UpdateHandleResult> Handle(CallbackQuery query, CancellationToken token)
    {
        var data = query.Data ?? string.Empty;
        bool isAccept;
        string prefix;

        if (data.StartsWith(DuelRequestMessage.AcceptCallbackDataPrefix))
        {
            isAccept = true;
            prefix = DuelRequestMessage.AcceptCallbackDataPrefix;
        }
        else if (data.StartsWith(DuelRequestMessage.DeclineCallbackDataPrefix))
        {
            isAccept = false;
            prefix = DuelRequestMessage.DeclineCallbackDataPrefix;
        }
        else
        {
            return UpdateHandleResult.CallbackQueryUnknownData;
        }

        if (!int.TryParse(data[prefix.Length..], out var duelRequestId))
        {
            await answerer.Answer(query.Id, "Не понял, что это за кнопка");
            return UpdateHandleResult.CallbackQueryInvalidDuelRequestId;
        }

        var duelRequest = await context.DuelRequests.AsTracking().FirstOrDefaultAsync(dr => dr.RequestId == duelRequestId, token);
        if (duelRequest is null)
        {
            await answerer.Answer(query.Id, "Этот вызов уже неактуален");
            return UpdateHandleResult.CallbackQueryDuelRequestNotFound;
        }

        var attackerSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == duelRequest.AttackerId, token);
        var defenderSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == duelRequest.DefenderId, token);
        var defender = await context.Users.FirstAsync(u => u.UserId == defenderSwine.OwnerId, token);

        // Buttons are visible to the whole group, but only the owner of the defending swine may answer
        if (defender.TelegramId != query.From.Id)
        {
            await answerer.Answer(query.Id, "Ответить на вызов может только владелец вызванного свина");
            return UpdateHandleResult.CallbackQueryDuelNotYourCall;
        }

        var utcNow = dtProvider.UtcNow;

        // Игнор вызова дольше TTL — это отказ: срабатывает дебафф за отказ
        if (duelRequest.IsExpired(utcNow))
        {
            var payment = await DuelDecline.ApplyAsync(context, duelRequest, utcNow, "ignored duel request");
            await context.SaveChangesAsync(token);

            await answerer.Answer(query.Id, "Вызов устарел, это засчитано как отказ");
            await EditRequestMessage<DuelResponseMessage, DuelResponseViewModel>(duelRequest, defenderSwine.GroupId,
                new DuelResponseViewModel(DuelResponseStatus.Expired, attackerSwine.Name, defenderSwine.Name, 0, payment), token);

            logger.LogInformation("Duel request [{requestId}] expired: {payment}", duelRequest.RequestId, payment);
            return UpdateHandleResult.CallbackQueryDuelRequestExpired;
        }

        return isAccept
            ? await Accept(query.Id, duelRequest, attackerSwine, defenderSwine, utcNow, token)
            : await Decline(query.Id, duelRequest, attackerSwine, defenderSwine, utcNow, token);
    }

    private async Task<UpdateHandleResult> Accept(string callbackQueryId, DuelRequest duelRequest, Swine attackerSwine, Swine defenderSwine, DateTime utcNow, CancellationToken token)
    {
        var attackerStats = await statsProvider.GetAsync(attackerSwine, utcNow);
        var defenderStats = await statsProvider.GetAsync(defenderSwine, utcNow);

        var outcome = DuelResolution.Resolve(attackerSwine.Weight, defenderSwine.Weight, Random.Shared.NextDouble(),
            attackerStats, defenderStats);

        // Слава начисляется за победу, а не за согласие: апсет (побил вдвое более тяжёлого)
        // приносит дополнительное очко
        var winnerSwine = outcome.AttackerWon ? attackerSwine : defenderSwine;
        var gloryReward = DuelResolution.GetGloryReward(outcome.AttackerWon,
            outcome.WinnerWeightBefore, outcome.LoserWeightBefore);
        winnerSwine.Glory = Math.Clamp(winnerSwine.Glory + gloryReward, DuelBalance.MinGlory, DuelBalance.MaxGlory);

        attackerSwine.Weight = outcome.AttackerWon ? outcome.WinnerWeightAfter : outcome.LoserWeightAfter;
        defenderSwine.Weight = outcome.AttackerWon ? outcome.LoserWeightAfter : outcome.WinnerWeightAfter;

        context.DuelRequests.Remove(duelRequest);
        context.DuelResults.Add(new DuelResult
        {
            AttackerId = attackerSwine.SwineId,
            DefenderId = defenderSwine.SwineId,
            GroupId = defenderSwine.GroupId,
            AttackerWon = outcome.AttackerWon,
            DateTime = utcNow,
            WinnerWeightBefore = outcome.WinnerWeightBefore,
            WinnerWeightAfter = outcome.WinnerWeightAfter,
            LoserWeightBefore = outcome.LoserWeightBefore,
            LoserWeightAfter = outcome.LoserWeightAfter,
        });

        await context.SaveChangesAsync(token);

        await EditRequestMessage<DuelResultMessage, DuelResultViewModel>(duelRequest, defenderSwine.GroupId, new DuelResultViewModel(
            attackerSwine.Name,
            defenderSwine.Name,
            outcome.AttackerWon,
            outcome.WinnerWeightBefore,
            outcome.WinnerWeightAfter,
            outcome.LoserWeightBefore,
            outcome.LoserWeightAfter), token);

        await answerer.Answer(callbackQueryId, outcome.AttackerWon ? "Ваш свин проиграл" : "Ваш свин победил!");

        logger.LogInformation("Duel request [{requestId}] accepted: attacker won={attackerWon}, stake={stake} kg, bonus {attackerBonus}/{defenderBonus}",
            duelRequest.RequestId, outcome.AttackerWon, outcome.StakeKg,
            DuelResolution.GetCombatBonus(attackerStats), DuelResolution.GetCombatBonus(defenderStats));

        return UpdateHandleResult.CallbackQueryOK;
    }

    private async Task<UpdateHandleResult> Decline(string callbackQueryId, DuelRequest duelRequest, Swine attackerSwine, Swine defenderSwine, DateTime utcNow, CancellationToken token)
    {
        var payment = await DuelDecline.ApplyAsync(context, duelRequest, utcNow, "duel declined");

        await context.SaveChangesAsync(token);

        await EditRequestMessage<DuelResponseMessage, DuelResponseViewModel>(duelRequest, defenderSwine.GroupId,
            new DuelResponseViewModel(DuelResponseStatus.Declined, attackerSwine.Name, defenderSwine.Name, 0, payment), token);

        await answerer.Answer(callbackQueryId, "Вызов отклонён");

        logger.LogInformation("Duel request [{requestId}] declined: {payment}", duelRequest.RequestId, payment);

        return UpdateHandleResult.CallbackQueryOK;
    }

    /// <summary>
    /// Replaces the request message with its outcome and drops the buttons. Best effort:
    /// a deleted or too old message must not break the outcome itself.
    /// </summary>
    private async Task EditRequestMessage<TMessage, TViewModel>(DuelRequest duelRequest, int? groupId, TViewModel viewModel, CancellationToken token)
        where TMessage : BotMessage<TViewModel>, new()
        where TViewModel : ViewModel
    {
        if (groupId is not { } gid || duelRequest.MessageId is not { } messageId)
            return;

        var group = await context.Groups.FirstOrDefaultAsync(g => g.GroupId == gid, token);
        if (group is null)
        {
            logger.LogError("Failed to edit duel request message [{messageId}], group [{groupId}] not found", messageId, gid);
            return;
        }

        var message = messageFactory.Create<TMessage, TViewModel>(viewModel);

        try
        {
            await messageEditor.EditMessage(group.TelegramId, messageId, message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to edit duel request message [{messageId}] in chat [{chatId}]", messageId, group.TelegramId);
        }
    }
}
