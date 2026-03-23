using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class NoOverfeedAchievementChecker(ILogger<NoOverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.NoOverfeed;

    public static int CountConsecutiveNoOverfeeds(UserContext context, int swineId)
    {
        var lastThrowUp = context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefault();

        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var recentFeeds = context.Feeds
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            .ToList();

        int noOverfeedCount = 0;
        for (int i = 0; i < recentFeeds.Count - 1; i++)
        {
            noOverfeedCount = i;

            var feed0 = recentFeeds[i];
            var feed1 = recentFeeds[i + 1];
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours < FeedGenerator.OVERFEED_COOLDOWN)
                break;
        }
        
        return noOverfeedCount;
    }

    protected override int? GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var noOverfeedCount = CountConsecutiveNoOverfeeds(context, swineId);
        return noOverfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}




