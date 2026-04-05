using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class NoOverfeedAchievementChecker(ILogger<NoOverfeedAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.NoOverfeed;

    public static async Task<int> CountConsecutiveNoOverfeeds(UserContext context, int? swineId)
    {
        var lastThrowUp = context.WeightLosses
            .AsNoTracking()
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefault();

        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var feedsSinceThrowup = await context.Feeds
            .AsNoTracking()
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            .ToListAsync();

        int noOverfeedCount = 0;
        for (int i = 0; i < feedsSinceThrowup.Count - 1; i++)
        {
            noOverfeedCount = i;

            var feed0 = feedsSinceThrowup[i];
            var feed1 = feedsSinceThrowup[i + 1];
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours < FeedGenerator.OVERFEED_COOLDOWN)
                break;
        }
        
        return noOverfeedCount;
    }

    protected override async Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var noOverfeedCount = await CountConsecutiveNoOverfeeds(context, swineId);
        return noOverfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}




