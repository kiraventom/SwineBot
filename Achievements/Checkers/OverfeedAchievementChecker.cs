using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class OverfeedAchievementChecker(IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(levels)
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
        var lastThrowUp = swine.WeightLosses.Where(wl => wl.IsThrowUp).MaxBy(wl => wl.DateTime);
        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var recentFeeds = swine.Feeds.Where(f => f.DateTime > dateToCountFrom).OrderByDescending(f => f.DateTime).ToList();
        int overfeedCount = 0;
        for (int i = 0; i < recentFeeds.Count - 1; i++)
        {
            var feed0 = recentFeeds[i];
            Log.Warning("Overfeed: feed0[{index}] date {date} amount {amount}", i, feed0.DateTime.ToLongDateString(), feed0.Amount);
            var feed1 = recentFeeds[i + 1];
            Log.Warning("Overfeed: feed1[{index}] date {date} amount {amount}", i+1, feed1.DateTime.ToLongDateString(), feed1.Amount);
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours >= FeedMessage.OVERFEED_COOLDOWN)
                break;

            overfeedCount = i + 1;
        }
        
        Log.Warning("Overfeed: count {count}", overfeedCount);
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
        Log.Warning("Overfeed: value {value}, level {level}", value, level);

        return value >= level;
    }

    protected override bool IsSilentApply(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
        {
            Log.Error("{botMessage} is not {FeedMessage}", nameof(botMessage), nameof(FeedMessage));
            return true;
        }

        Log.Information("OVERFEED: {swineWeight} {newWeight}", swine.Weight, feedMessage.NewWeight);
        return swine.Weight != feedMessage.NewWeight;
    }
}
