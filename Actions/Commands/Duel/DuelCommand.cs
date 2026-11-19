using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using Microsoft.Extensions.Logging;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo("/duel", "Вызвать на дуэль \u2694\uFE0F")]
public class DuelCommand(ICommandFactory commandFactory, UserContext context, IMessageFactory messageFactory, IDateTimeNowProvider dtProvider, DuelStatsProvider statsProvider) : ICommand, IActionCommand
{
    public async Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter = null)
    {
        // Вызов можно отправлять не чаще раза в сутки (на свина)
        var sendSince = dtProvider.UtcNow.AddHours(-DuelBalance.SendCooldownHours);
        var sentRecently = await context.DuelResults
            .AnyAsync(r => r.DateTime > sendSince && r.AttackerId == update.SwineId);

        if (sentRecently)
        {
            var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
            var cooldownVm = new TooSoonToDuelViewModel(swine.Name, swine.Name, DuelBalance.SendCooldownHours, false);
            return [ messageFactory.Create<TooSoonToDuelMessage, TooSoonToDuelViewModel>(cooldownVm) ];
        }

        // Дуэль — событие для беседы, а не поток: один вызов на беседу в сутки
        if (await HasRecentCallInGroup(update.SwineId))
        {
            var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
            var groupVm = new TooSoonToDuelViewModel(swine.Name, swine.Name, DuelBalance.CallCooldownHours, false);
            return [ messageFactory.Create<TooSoonToDuelMessage, TooSoonToDuelViewModel>(groupVm) ];
        }

        if (!string.IsNullOrEmpty(parameter) && long.TryParse(parameter, out var opponentTgId))
            return await SendDuelRequest(update, opponentTgId);
        else
            return await SendSelectOpponent(update);
    }

    /// <summary>
    /// Был ли в беседе вызов за последние <see cref="DuelBalance.CallCooldownHours"/> часов:
    /// свежий результат дуэли или висящий запрос. Дуэль — событие, а не поток.
    /// </summary>
    private async Task<bool> HasRecentCallInGroup(int? swineId)
    {
        if (swineId is not { } id)
            return false;

        var swine = await context.Swines.FirstAsync(s => s.SwineId == id);
        if (swine.GroupId is not { } groupId)
            return false;

        var since = dtProvider.UtcNow.AddHours(-DuelBalance.CallCooldownHours);
        if (await context.DuelResults.AnyAsync(r => r.GroupId == groupId && r.DateTime > since))
            return true;

        var requestTtlStart = dtProvider.UtcNow - DuelRequestExtensions.TTL;
        return await context.DuelRequests
            .Join(context.Swines, dr => dr.DefenderId, s => s.SwineId, (dr, s) => new { dr.DateTime, s.GroupId })
            .AnyAsync(x => x.GroupId == groupId && x.DateTime > requestTtlStart);
    }

    private async Task<IReadOnlyCollection<IBotMessage>> SendSelectOpponent(Update update)
    {
        var user = await context.Users.FirstAsync(u => u.UserId == update.UserId);

        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);

        string existingDuelRequestOpponentName = null;
        var existingDuelRequest = await context.GetActiveOutgoingDuelRequest(swine.SwineId, dtProvider.UtcNow);
        if (existingDuelRequest is not null)
        {
            var existingDuelRequestOpponent = await context.Swines.FirstOrDefaultAsync(s => s.SwineId == existingDuelRequest.DefenderId);
            existingDuelRequestOpponentName = existingDuelRequestOpponent.Name;
        }

        var isPrivate = update.IsPrivateChat;
        int groupId;

        if (isPrivate)
        {
            var privateSwineId = user.PrivateSwineId;
            var privateSwine = await context.Swines.FirstAsync(s => s.SwineId == privateSwineId);
            groupId = privateSwine.GroupId.Value;
        }
        else
        {
            groupId = update.GroupId.Value;
        }

        var opponents = await context.GetOpponents(user.UserId, groupId, dtProvider.UtcNow);
        var opponentsCount = opponents.Count();

        var group = await context.Groups.FirstAsync(g => g.GroupId == groupId);

        var viewModel = new SelectDuelOpponentViewModel(group.TelegramId.ToString(), group.Title, opponentsCount, existingDuelRequestOpponentName);
        return [ messageFactory.Create<SelectDuelOpponentMessage, SelectDuelOpponentViewModel>(viewModel) ];
    }

    private async Task<IReadOnlyCollection<IBotMessage>> SendDuelRequest(Update update, long opponentTgId)
    {
        var user = await context.Users.FirstAsync(u => u.UserId == update.UserId);
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);

        int groupId;
        if (update.IsPrivateChat)
        {
            var privateSwineId = user.PrivateSwineId;
            var privateSwine = await context.Swines.FirstAsync(s => s.SwineId == privateSwineId);
            groupId = privateSwine.GroupId.Value;
        }
        else
        {
            groupId = update.GroupId.Value;
        }

        var opponents = await context.GetOpponents(user.UserId, groupId, dtProvider.UtcNow);

        var opponent = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == opponentTgId);
        if (opponent is null)
            throw new NotSupportedException($"User with TelegramId={opponentTgId} not found");

        var opponentSwine = await context.Swines
            .Where(s => s.OwnerId == opponent.UserId)
            .FirstOrDefaultAsync(s => s.GroupId == groupId);

        if (opponentSwine is null)
            throw new NotSupportedException($"User {opponent.UserId} does not have swines in the group {groupId}");

        // Свина можно вызывать не чаще раза в двое суток — но на труса (слава в минусе)
        // можно напасть без очереди: у отказа есть цена не только в очках славы
        var defendSince = dtProvider.UtcNow.AddHours(-DuelBalance.DefendCooldownHours);
        var defendedRecently = opponentSwine.Glory >= 0 && await context.DuelResults
            .AnyAsync(r => r.DateTime > defendSince && r.DefenderId == opponentSwine.SwineId);

        if (defendedRecently)
        {
            var defendVm = new TooSoonToDuelViewModel(swine.Name, opponentSwine.Name, DuelBalance.DefendCooldownHours, false);
            return [ messageFactory.Create<TooSoonToDuelMessage, TooSoonToDuelViewModel>(defendVm) ];
        }

        // Ту же пару нельзя дёргать чаще, чем раз в 72 часа
        var pairSince = dtProvider.UtcNow.AddHours(-DuelBalance.PairCooldownHours);
        var pairRecently = await context.DuelResults
            .AnyAsync(r => r.DateTime > pairSince && r.AttackerId == swine.SwineId && r.DefenderId == opponentSwine.SwineId);

        if (pairRecently)
        {
            var pairVm = new TooSoonToDuelViewModel(swine.Name, opponentSwine.Name, DuelBalance.PairCooldownHours, false);
            return [ messageFactory.Create<TooSoonToDuelMessage, TooSoonToDuelViewModel>(pairVm) ];
        }

        if (await context.GetActiveIncomingDuelRequest(opponentSwine.SwineId, dtProvider.UtcNow) is {} existingDuelRequest)
            throw new NotSupportedException($"Swine {opponentSwine.SwineId} was selected as opponent despite already being a defender in duel request {existingDuelRequest.RequestId}");

        var duelRequestId = await GetDuelRequestId(update, swine, opponents, opponentSwine);

        var attackerStats = await statsProvider.GetAsync(swine, dtProvider.UtcNow);
        var defenderStats = await statsProvider.GetAsync(opponentSwine, dtProvider.UtcNow);

        var winChance = DuelResolution.GetDefenderWinChance(swine.Weight, opponentSwine.Weight, attackerStats, defenderStats);
        var stakeKg = DuelResolution.GetStakeKg(swine.Weight, opponentSwine.Weight);
        var attackerBonus = DuelResolution.GetCombatBonus(attackerStats);
        var defenderBonus = DuelResolution.GetCombatBonus(defenderStats);

        var viewModel = new DuelRequestViewModel(duelRequestId, opponent.FirstName, opponentSwine.Name, opponentSwine.Weight, opponent.Tag, opponent.TelegramId, user.FirstName, swine.Name, swine.Weight, winChance, (int)Math.Round(DuelBalance.StakeShare * 100), stakeKg, 0, attackerBonus, defenderBonus);

        List<IBotMessage> messagesToSend = [];

        var duelRequestMessage = messageFactory.Create<DuelRequestMessage, DuelRequestViewModel>(viewModel);
        messagesToSend.Add(duelRequestMessage);

        // /duel command was sent as PM. We send the request message in the group and return with confirmation to PM
        // We do not send duel requests in private messages because not everyone has private chat with bot
        if (update.IsPrivateChat)
        {
            duelRequestMessage.CustomRecepient = Recepient.Group(context, opponentSwine.GroupId.Value);

            var duelRequestSentViewModel = new DuelRequestSentViewModel(opponent.FirstName, opponentSwine.Name);
            var duelRequestSentMessage = messageFactory.Create<DuelRequestSentMessage, DuelRequestSentViewModel>(duelRequestSentViewModel);

            messagesToSend.Add(duelRequestSentMessage);
        }

        return messagesToSend;
    }

    private async Task<int> GetDuelRequestId(Update update, Swine swine, IReadOnlyList<PotentialOpponent> opponents, Swine opponentSwine)
    {
        // Invalid opponent (old message, manually entered parameter)
        if (opponents.All(o => o.Swine.SwineId != opponentSwine.SwineId))
            throw new NotSupportedException($"Swine {opponentSwine.SwineId} is not valid opponent for {swine.SwineId}");

        var existingDuelRequest = await context.GetActiveOutgoingDuelRequest(swine.SwineId, dtProvider.UtcNow);
        if (existingDuelRequest is not null)
        {
            // TODO Stupid code
            var duelCancelCommand = commandFactory.Create<DuelCancelCommand>();
            await duelCancelCommand.ExecuteSilent(update);
        }

        var duelRequest = new DuelRequest()
        {
            DateTime = dtProvider.UtcNow,
            AttackerId = swine.SwineId,
            DefenderId = opponentSwine.SwineId
        };

        context.DuelRequests.Add(duelRequest);
        await context.SaveChangesAsync();

        return duelRequest.RequestId;
    }

    async Task ICommand.AfterMessageSend(Update update, IBotMessage message, Telegram.Bot.Types.Message sentMessage)
    {
        if (message is DuelRequestMessage)
        {
            var duelRequest = await context.DuelRequests.AsTracking().FirstOrDefaultAsync(dr => dr.AttackerId == update.SwineId);

            if (duelRequest is { MessageId: null })
            {
                duelRequest.MessageId = sentMessage.MessageId;
            }
        }
    }
}


