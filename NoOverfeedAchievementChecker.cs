using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements;

public class NoOverfeedAchievementChecker(IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(levels)
{
    protected override AchievementType AchievementType => AchievementType.NoOverfeed;

    protected override CheckerResult CheckLevel(BotMessage botMessage, Swine swine, int levelValue)
    {
        if (botMessage is not FeedMessage feedMessage)
            return CheckerResult.Break;

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
            if (offset.TotalHours < 24)
                break;
        }

        if (noOverfeedCount >= levelValue) // Checking if level applies
        {
            if (swine.Weight == feedMessage.NewWeight) // Reacting only to last feed
            {
                return CheckerResult.Apply;
            }

            return CheckerResult.ApplySilent; // levelValue is not too high, but weight was not achieved in last feed
        }

        return CheckerResult.Continue; // levelValue too high
    }
}




