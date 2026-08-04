using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo(COMMAND_NAME, "Получить инфу о своём свине \u2139\ufe0f")]
public class InfoCommand(UserContext context, IDateTimeNowProvider dtnProvider, IMessageFactory messageFactory, AchievementController achievController) : Command<InfoMessage, InfoViewModel>(messageFactory, achievController)
{
    public const string COMMAND_NAME = "/info";

    protected override async Task<InfoViewModel> ExecuteInternal(Update update, string parameter)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);

        var duels = context.DuelResults;
        var wonDuels = await duels.CountAsync(d => (d.AttackerWon && d.AttackerId == update.SwineId || !d.AttackerWon && d.DefenderId == update.SwineId));
        var lostDuels = await duels.CountAsync(d => (d.AttackerWon && d.DefenderId == update.SwineId || !d.AttackerWon && d.AttackerId == update.SwineId));

        var utcNow = dtnProvider.UtcNow;

        var recentFeeds = await context.GetRecentFeeds(update.SwineId, utcNow);
        var recentThrowups = await context.GetRecentThrowups(update.SwineId, utcNow);

        int consecutiveOverfeeds = await OverfeedChecker.CountConsecutiveOverfeeds(context, update.SwineId);
        int consecutiveNoOverfeeds = await NoOverfeedChecker.CountConsecutiveNoOverfeeds(context, update.SwineId);

        var owner = await context.Users.FirstAsync(u => u.UserId == swine.OwnerId);

        var slaughters = context.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .Where(s => s.GroupId == swine.GroupId);

        var effectiveSlaughters = slaughters
            .Where(s => s.SwineWeight >= SlaughterCommand.MIN_SWINE_WEIGHT);

        var totalSlaughteredWeight = await slaughters.CountAsync() > 0 ? await effectiveSlaughters.SumAsync(s => s.SwineWeight) : 0;

        var slaughtersCount = await slaughters.CountAsync();

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        Swine outcomingDuelTarget = null;
        var outcomingDuelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.AttackerId == swine.SwineId);
        if (outcomingDuelRequest is not null)
            outcomingDuelTarget = await context.Swines.FirstOrDefaultAsync(s => s.SwineId == outcomingDuelRequest.DefenderId);

        Swine incomingDuelSource = null;
        var incomingDuelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.DefenderId == swine.SwineId);
        if (incomingDuelRequest is not null)
            incomingDuelSource = await context.Swines.FirstOrDefaultAsync(s => s.SwineId == incomingDuelRequest.AttackerId);

        return new InfoViewModel(recentFeeds.Select(f => f.DateTime).ToList(), recentThrowups.Select(t => t.DateTime).ToList(), utcNow, owner, swine, consecutiveOverfeeds, consecutiveNoOverfeeds, 0, 0, slaughtersCount, (int)Math.Round((growthMod - 1) * 100), outcomingDuelTarget?.Name, incomingDuelSource?.Name);
    }
}
