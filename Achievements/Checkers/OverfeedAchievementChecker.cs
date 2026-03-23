using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class OverfeedAchievementChecker(ILogger<OverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.Overfeed;

    public static int CountConsecutiveOverfeeds(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.SwineId == swineId);

        return CountConsecutiveOverfeeds(swine);
    }

    private static int CountConsecutiveOverfeeds(Swine swine)
    {
        var throwUps = swine.WeightLosses.Where(wl => wl.IsThrowUp);
        var lastThrowUp = throwUps.Where(wl => !wl.Ignored).MaxBy(wl => wl.DateTime);
        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var recentFeeds = swine.Feeds.Where(f => f.DateTime > dateToCountFrom).OrderByDescending(f => f.DateTime).ToList();
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

    protected override int? GetValue(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var overfeedCount = CountConsecutiveOverfeeds(swine);
        return overfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level)
    { 
        Logger.LogDebug("Overfeed: value {value}, level {level}", value, level);

        return value >= level;
    }

    protected override bool IsSilentApply(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
        {
            Logger.LogError("{botMessage} is not {FeedMessage}", nameof(botMessage), nameof(FeedMessage));
            return true;
        }

        Logger.LogInformation("OVERFEED: {swineWeight} {newWeight}", swine.Weight, feedMessage.FeedResult.NewWeight);
        return swine.Weight != feedMessage.FeedResult.NewWeight;
    }
}
