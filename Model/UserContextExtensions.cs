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

    public static async Task<IReadOnlyList<PotentialOpponent>> GetOpponents(this UserContext context, int userId, int groupId, DateTime utcNow)
    {
        var requestTtlStart = utcNow - DuelRequestExtensions.TTL;
        var busySwinesIds = context.DuelRequests.Where(dr => dr.DateTime > requestTtlStart).Select(dr => dr.DefenderId);

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

    public static async Task<DuelRequest> GetActiveIncomingDuelRequest(this UserContext context, int? swineId, DateTime utcNow)
    {
        if (swineId is null)
            return null;

        var requests = await context.DuelRequests
            .AsTracking()
            .Where(dr => dr.DefenderId == swineId)
            .ToListAsync();

        return PickActiveRequest(context, requests, utcNow);
    }

    public static async Task<DuelRequest> GetActiveOutgoingDuelRequest(this UserContext context, int swineId, DateTime utcNow)
    {
        var requests = await context.DuelRequests
            .AsTracking()
            .Where(dr => dr.AttackerId == swineId)
            .ToListAsync();

        return PickActiveRequest(context, requests, utcNow);
    }

    private static DuelRequest PickActiveRequest(UserContext context, IReadOnlyCollection<DuelRequest> requests, DateTime utcNow)
    {
        // Просроченные не удаляем здесь: игнор вызова — это отказ, а штраф за отказ
        // накладывает DuelExpiry, чтобы он применялся ровно один раз
        return requests
            .Where(r => !r.IsExpired(utcNow))
            .OrderByDescending(r => r.DateTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Вес забитых предшественников плюс поправки от дуэлей — из него считается
    /// множитель кормления.
    /// </summary>
    public static async Task<int> GetSlaughterWeightAsync(this UserContext context, int userId, int? groupId)
    {
        var slaughtered = await context.Slaughters
            .Where(s => s.UserId == userId && s.GroupId == groupId)
            .SumAsync(s => (int?)s.SwineWeight) ?? 0;

        var bonus = await context.SlaughterBonuses
            .Where(b => b.UserId == userId && b.GroupId == groupId)
            .SumAsync(b => (int?)b.Weight) ?? 0;

        return Math.Max(0, slaughtered + bonus);
    }

    /// <summary>Сколько свинов владелец забил в этой группе — дебафф в дуэлях.</summary>
    public static Task<int> GetKillsAsync(this UserContext context, int userId, int? groupId)
        => context.Slaughters.CountAsync(s => s.UserId == userId && s.GroupId == groupId);
}

