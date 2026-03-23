using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class OverfeedAchievementChecker(ILogger<OverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.Overfeed;

    public static int CountConsecutiveOverfeeds(UserContext context, int swineId)
    {
        var throwUps = context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .ToList();

        var lastThrowUp = throwUps
            .Where(wl => !wl.Ignored)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefault();

        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;

        var recentFeeds = context.Feeds
            .Where(wl => wl.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            .ToList();

        int overfeedCount = 0;
        for (int i = 0; i < recentFeeds.Count - 1; i++)
        {
            var newerFeed = recentFeeds[i];
            var olderFeed = recentFeeds[i + 1];
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

    protected override int? GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var overfeedCount = CountConsecutiveOverfeeds(context, swineId);
        return overfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level)
    { 
        Logger.LogDebug("Overfeed: value {value}, level {level}", value, level);

        return value >= level;
    }
}
