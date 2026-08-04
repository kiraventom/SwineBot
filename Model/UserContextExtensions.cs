using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using SwineBot.BotMessages.Feed;

namespace SwineBot.Model;

public static class UserContextExtensions
{
    public static async Task<IReadOnlyList<Feed>> GetRecentFeeds(this UserContext context, int? swineId, DateTime utcNow)
    {
        var dateToCountFeedsFrom = utcNow.AddHours(FeedGenerator.OVERFEED_COOLDOWN * -1);

        return await context.Feeds
            
            .Where(f => f.SwineId == swineId)
            .Where(f => f.DateTime > dateToCountFeedsFrom)
            .ToListAsync();
    }

    public static async Task<IReadOnlyList<WeightLoss>> GetRecentThrowups(this UserContext context, int? swineId, DateTime utcNow)
    {
        var dateToCountThrowupsFrom = utcNow.AddHours(FeedGenerator.THROWUP_COOLDOWN * -1);

        return await context.WeightLosses
            .Where(wl => wl.SwineId == swineId)
            .Where(wl => wl.IsThrowUp)
            // TODO this and similar should be changed to OrderByDescending + TakeWhile
            .Where(f => f.DateTime > dateToCountThrowupsFrom)
            .ToListAsync();
    }

    public static async Task<IReadOnlyList<PotentialOpponent>> GetOpponents(this UserContext context, int userId, int groupId)
    {
        var busySwinesIds = context.DuelRequests.Select(dr => dr.DefenderId);

        var members = await context.Swines
            .Where(s => s.GroupId == groupId)
            .Where(s => s.OwnerId != userId)
            .Where(s => s.Weight > 1)
            .Where(s => !busySwinesIds.Contains(s.SwineId))
            .OrderByDescending(s => s.Weight)
            .Join(context.Users, s => s.OwnerId, u => u.UserId, 
                    (s, u) => new { Owner = u, Swine = s })
            .Take(50)
            .ToListAsync();

        var opps = members
            .Select(x => new PotentialOpponent(x.Owner, x.Swine))
            .ToList();

        return opps;
    }
}

