using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Feed;

namespace SwineBot.Model;

public static class UserContextExtensions
{
    public static async Task<IReadOnlyList<Feed>> GetRecentFeeds(this UserContext context, int? swineId, DateTime utcNow)
    {
        var dateToCountFeedsFrom = utcNow.AddHours(FeedGenerator.OVERFEED_COOLDOWN * -1);

        return await context.Feeds
            .AsNoTracking()
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFeedsFrom)
            .ToListAsync();
    }

    public static async Task<IReadOnlyList<WeightLoss>> GetRecentThrowups(this UserContext context, int? swineId, DateTime utcNow)
    {
        var dateToCountThrowupsFrom = utcNow.AddHours(FeedGenerator.THROWUP_COOLDOWN * -1);

        return await context.WeightLosses
            .AsNoTracking()
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            .Where(f => f.DateTime > dateToCountThrowupsFrom)
            .ToListAsync();
    }
}

