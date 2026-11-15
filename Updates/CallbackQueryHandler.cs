using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Duels;
using SwineBot.Model;
using SwineBot.Senders;
using SwineBot.ViewModels;
using Telegram.Bot.Types;

namespace SwineBot.Updates;

public class CallbackQueryHandler(ILogger<CallbackQueryHandler> logger, UserContext context, IMessageEditor messageEditor, ICallbackQueryAnswerer answerer, IMessageFactory messageFactory, IDateTimeNowProvider dtProvider) : UpdateTypeHandler<CallbackQuery>
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

        var attackerSwine = await context.Swines.FirstAsync(s => s.SwineId == duelRequest.AttackerId, token);
        var defenderSwine = await context.Swines.FirstAsync(s => s.SwineId == duelRequest.DefenderId, token);
        var defender = await context.Users.FirstAsync(u => u.UserId == defenderSwine.OwnerId, token);

        // Buttons are visible to the whole group, but only the owner of the defending swine may answer
        if (defender.TelegramId != query.From.Id)
        {
            await answerer.Answer(query.Id, "Ответить на вызов может только владелец вызванного свина");
            return UpdateHandleResult.CallbackQueryDuelNotYourCall;
        }

        var status = duelRequest.IsExpired(dtProvider.UtcNow)
            ? DuelResponseStatus.Expired
            : isAccept
                ? DuelResponseStatus.Accepted
                : DuelResponseStatus.Declined;

        // TODO Duel outcome (winner, weight changes, decline penalty) requires balance rules, it is a separate task
        context.DuelRequests.Remove(duelRequest);

        await answerer.Answer(query.Id, status switch
        {
            DuelResponseStatus.Expired => "Вызов устарел",
            DuelResponseStatus.Accepted => "Вызов принят",
            _ => "Вызов отклонён",
        });

        await EditRequestMessage(duelRequest, attackerSwine, defenderSwine, status, token);

        await context.SaveChangesAsync(token);

        logger.LogInformation("Duel request [{requestId}] resolved as {status} by user [{userId}]", duelRequest.RequestId, status.ToString(), query.From.Id);

        return status is DuelResponseStatus.Expired
            ? UpdateHandleResult.CallbackQueryDuelRequestExpired
            : UpdateHandleResult.CallbackQueryOK;
    }

    /// <summary>
    /// Replaces the request message with its outcome and drops the buttons. Best effort:
    /// a deleted or too old message must not break the answer itself.
    /// </summary>
    private async Task EditRequestMessage(DuelRequest duelRequest, Swine attackerSwine, Swine defenderSwine, DuelResponseStatus status, CancellationToken token)
    {
        if (defenderSwine.GroupId is not { } groupId || duelRequest.MessageId is not { } messageId)
            return;

        var group = await context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId, token);
        if (group is null)
        {
            logger.LogError("Failed to edit duel request message [{messageId}], group [{groupId}] not found", messageId, groupId);
            return;
        }

        var viewModel = new DuelResponseViewModel(status, attackerSwine.Name, defenderSwine.Name);
        var message = messageFactory.Create<DuelResponseMessage, DuelResponseViewModel>(viewModel);

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
