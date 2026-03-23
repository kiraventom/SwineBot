using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class NoOverfeedAchievementChecker(ILogger<NoOverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.NoOverfeed;

    public static int CountConsecutiveNoOverfeeds(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.SwineId == swineId);

        return CountConsecutiveNoOverfeeds(swine);
    }

    private static int CountConsecutiveNoOverfeeds(Swine swine)
    {
        var lastThrowUp = swine.WeightLosses.Where(wl => wl.IsThrowUp).MaxBy(wl => wl.DateTime);
        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var recentFeeds = swine.Feeds.Where(f => f.DateTime > dateToCountFrom).OrderByDescending(f => f.DateTime).ToList();
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

    protected override int? GetValue(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var noOverfeedCount = CountConsecutiveNoOverfeeds(swine);
        return noOverfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;


    protected override bool IsSilentApply(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
        {
            Logger.LogError("{botMessage} is not {FeedMessage}", nameof(botMessage), nameof(FeedMessage));
            return true;
        }

        return swine.Weight != feedMessage.FeedResult.NewWeight;
    }
}




