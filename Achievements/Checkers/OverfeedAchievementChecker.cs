using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class OverfeedAchievementChecker(ILogger<OverfeedAchievementChecker> Logger, UserContext context, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger,  dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.Overfeed;

    public static async Task<int> CountConsecutiveOverfeeds(UserContext context, int? swineId)
    {
        var throwUps = await context.WeightLosses
            .AsNoTracking()
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .ToListAsync();

        var lastActualThrowUp = throwUps
            .Where(wl => !wl.Ignored)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefault();

        var dateToCountFrom = lastActualThrowUp?.DateTime ?? DateTime.MinValue;

        var feedsSinceThrowup = await context.Feeds
            .AsNoTracking()
            .Where(wl => wl.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
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
                var ignoredThrowUp = throwUps
                    .Where(tu => tu.DateTime < newerFeed.DateTime)
                    .Where(tu => tu.DateTime > olderFeed.DateTime)
                    .Where(tu => tu.Ignored)
                    .FirstOrDefault();

                if (ignoredThrowUp is null)
                    break;
            }

            overfeedCount = i + 1;
        }
        
        return overfeedCount;
    }

    protected override async Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var overfeedCount = await CountConsecutiveOverfeeds(context, swineId);
        return overfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level)
    { 
        Logger.LogDebug("Overfeed: value {value}, level {level}", value, level);

        return value >= level;
    }
}
