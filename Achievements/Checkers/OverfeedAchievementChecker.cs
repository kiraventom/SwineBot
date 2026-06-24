using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class OverfeedChecker(ILogger<OverfeedChecker> Logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger,  dtnProvider, context)
{
    public override AchievementType Type => AchievementType.Overfeed;

    public static async Task<int> CountConsecutiveOverfeeds(UserContext Context, int? swineId)
    {
        var throwUps = Context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp);

        var ignoredThrowups = await throwUps
            .Where(t => t.Amount == 0)
            .ToListAsync();

        var lastActualThrowUp = await throwUps
            .Where(wl => wl.Amount != 0) // skip ignored
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefaultAsync();

        var dateToCountFrom = lastActualThrowUp?.DateTime ?? DateTime.MinValue;

        var feedsSinceThrowup = await Context.Feeds
            .Where(wl => wl.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            // TODO: Add Take({maximum level for this achievement}) here
            .ToListAsync();

        int overfeedCount = 0;
        for (int i = 0; i < feedsSinceThrowup.Count - 1; i++)
        {
            var newerFeed = feedsSinceThrowup[i];
            var olderFeed = feedsSinceThrowup[i + 1];
            var offset = newerFeed.DateTime - olderFeed.DateTime;
            if (offset.TotalHours >= FeedGenerator.OVERFEED_COOLDOWN)
            {
                // Check for ignored throwup
                var ignoredThrowUp = ignoredThrowups
                    .Where(tu => tu.DateTime < newerFeed.DateTime)
                    .Where(tu => tu.DateTime > olderFeed.DateTime)
                    .FirstOrDefault();

                if (ignoredThrowUp is null)
                    break;
            }

            overfeedCount = i + 1;
        }
        
        return overfeedCount;
    }

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel feedViewModel)
            return null;

        var overfeedCount = await CountConsecutiveOverfeeds(Context, swineId);
        return overfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level)
    { 
        Logger.LogDebug("Overfeed: value {value}, level {level}", value, level);

        return value >= level;
    }
}
