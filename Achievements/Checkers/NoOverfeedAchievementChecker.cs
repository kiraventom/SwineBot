using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class NoOverfeedAchievementChecker(ILogger<NoOverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.NoOverfeed;

    public static async Task<int> CountConsecutiveNoOverfeeds(UserContext context, int? swineId)
    {
        var lastThrowUp = await context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefaultAsync();

        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var feedsSinceThrowup = await context.Feeds
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            // TODO: JSONLEVELS: Add Take({maximum level for this achievement}) here
            .ToListAsync();

        int noOverfeedCount = 0;
        for (int i = 0; i < feedsSinceThrowup.Count - 1; i++)
        {
            var feed0 = feedsSinceThrowup[i];
            var feed1 = feedsSinceThrowup[i + 1];
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours < FeedGenerator.OVERFEED_COOLDOWN)
                break;

            ++noOverfeedCount;
        }
        
        return noOverfeedCount;
    }

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel)
            return null;

        var noOverfeedCount = await CountConsecutiveNoOverfeeds(context, swineId);
        return noOverfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}
