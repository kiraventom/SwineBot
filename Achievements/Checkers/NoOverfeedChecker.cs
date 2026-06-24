using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class NoOverfeedChecker(ILogger<NoOverfeedChecker> logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.NoOverfeed;

    public static async Task<int> CountConsecutiveNoOverfeeds(UserContext Context, int? swineId)
    {
        var lastThrowUp = await Context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefaultAsync();

        var dateToCountFrom = lastThrowUp?.DateTime ?? DateTime.MinValue;
        var feedsSinceThrowup = await Context.Feeds
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFrom)
            .OrderByDescending(f => f.DateTime)
            // TODO: Add Take({maximum level for this achievement}) here
            .ToListAsync();

        int noOverfeedCount = 0;
        for (int i = 0; i < feedsSinceThrowup.Count - 1; i++)
        {
            var feed0 = feedsSinceThrowup[i];
            var feed1 = feedsSinceThrowup[i + 1];
            var offset = feed0.DateTime - feed1.DateTime;
            if (offset.TotalHours < FeedGenerator.OVERFEED_COOLDOWN)
                break;

            ++noOverfeedCount;
        }
        
        return noOverfeedCount;
    }

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel)
            return null;

        var noOverfeedCount = await CountConsecutiveNoOverfeeds(Context, swineId);
        return noOverfeedCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}
