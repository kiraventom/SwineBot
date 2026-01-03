using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class OverfeedAchievementChecker(IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(levels)
{
    public override AchievementType Type => AchievementType.Overfeed;

    protected override int? GetValue(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var lastThrowUp = swine.WeightLosses.Where(wl => wl.IsThrowUp).MaxBy(wl => wl.DateTime);
        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var recentFeeds = swine.Feeds.Where(f => f.DateTime > dateToCountFrom).OrderByDescending(f => f.DateTime).ToList();
        int overfeedCount = 0;
        for (int i = 0; i < recentFeeds.Count - 1; i++)
        {
            overfeedCount = i;

            var feed0 = recentFeeds[i];
            var feed1 = recentFeeds[i + 1];
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours >= 24)
                break;
        }

        return overfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;

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
