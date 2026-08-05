using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using Microsoft.Extensions.Logging;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo("/duel", "Вызвать на дуэль \u2694\uFE0F")]
public class DuelCommand(ILogger<DuelCommand> logger, ICommandFactory commandFactory, UserContext context, IMessageFactory messageFactory, IDateTimeNowProvider dtProvider) : ICommand, IActionCommand
{
    private const int DUEL_COOLDOWN = 24;

    // If true, no changes to database will occur
    public bool ReminderMode => DuelRequestId is not null;
    public int? DuelRequestId { get; set; }

    public async Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter = null)
    {
        if (ReminderMode)
        {
            var duelRequest = await context.DuelRequests.FirstAsync(dr => dr.RequestId == DuelRequestId.Value);
            var attacker = await context.Swines.FirstAsync(s => s.SwineId == duelRequest.AttackerId);
            var defender = await context.Swines.FirstAsync(s => s.SwineId == duelRequest.DefenderId);
            var defenderOwner = await context.Users.FirstAsync(u => u.UserId == defender.OwnerId);
            
            // Replace update as if it was sent by attacker
            update = new Update(update.Text, update.GroupId, attacker.OwnerId, attacker.SwineId, update.TelegramChatId, update.IsPrivateChat);
            return await SendDuelRequest(update, defenderOwner.TelegramId);
        }

        var lastDuel = await context.DuelResults
            .Where(r => r.AttackerId == update.SwineId)
            .OrderByDescending(r => r.DateTime)
            .FirstOrDefaultAsync();

        if (lastDuel is not null)
        {
            var now = dtProvider.UtcNow;
            var hoursPassed = (now - lastDuel.DateTime).TotalHours;
            if (hoursPassed < DUEL_COOLDOWN)
            {
                var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
                var lastOpponent = await context.Swines.FirstAsync(s => s.SwineId == lastDuel.DefenderId);

                var viewModel = new TooSoonToDuelViewModel(swine.Name, lastOpponent.Name, DUEL_COOLDOWN - (int)hoursPassed);
                return [ messageFactory.Create<TooSoonToDuelMessage, TooSoonToDuelViewModel>(viewModel) ];
            }
        }

        if (!string.IsNullOrEmpty(parameter) && long.TryParse(parameter, out var opponentTgId))
            return await SendDuelRequest(update, opponentTgId);
        else
            return await SendSelectOpponent(update);
    }

    private async Task<IReadOnlyCollection<IBotMessage>> SendSelectOpponent(Update update)
    {
        var user = await context.Users.FirstAsync(u => u.UserId == update.UserId);

        string existingDuelRequestOpponentName = null;
        var existingDuelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.AttackerId == update.SwineId);
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

        var opponents = await context.GetOpponents(user.UserId, groupId);
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

        var opponents = await context.GetOpponents(user.UserId, groupId);

        var opponent = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == opponentTgId);
        if (opponent is null)
            throw new NotSupportedException($"User with TelegramId={opponentTgId} not found");

        var opponentSwine = await context.Swines
            .Where(s => s.OwnerId == opponent.UserId)
            .FirstOrDefaultAsync(s => s.GroupId == groupId);

        if (opponentSwine is null)
            throw new NotSupportedException($"User {opponent.UserId} does not have swines in the group {groupId}");

        var duelRequestId = await GetDuelRequestId(update, swine, opponents, opponentSwine);

        var winChance = (int)Math.Round(((double)opponentSwine.Weight / (swine.Weight + opponentSwine.Weight)) * 100);
        var declinePenalty = (int)Math.Round((winChance - 50) * 0.75);

        var viewModel = new DuelRequestViewModel(duelRequestId, opponent.FirstName, opponentSwine.Name, opponentSwine.Weight, opponent.Tag, opponent.TelegramId, user.FirstName, swine.Name, swine.Weight, winChance, declinePenalty, ReminderMode);

        List<IBotMessage> messagesToSend = [];

        var duelRequestMessage = messageFactory.Create<DuelRequestMessage, DuelRequestViewModel>(viewModel);
        messagesToSend.Add(duelRequestMessage);

        // If we're not in reminder mode, it means /duel command was sent as PM. We send the request message in the group and return with confirmation to PM
        if (update.IsPrivateChat && !ReminderMode)
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
        if (ReminderMode)
            return DuelRequestId.Value;

        // Invalid opponent (old message, manually entered parameter)
        if (opponents.All(o => o.Swine.SwineId != opponentSwine.SwineId))
            throw new NotSupportedException($"Swine {opponentSwine.SwineId} is not valid opponent for {swine.SwineId}");

        var existingDuelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.AttackerId == swine.SwineId);
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


