using Microsoft.EntityFrameworkCore;
using SwineBot.Model;

namespace SwineBot.Actions.Commands.Duel;

/// <summary>
/// Общая часть «дуэль не состоялась»: отказ, игнор вызова дольше TTL или забой свина
/// под вызовом. Во всех трёх случаях защитник отдаёт вызывающему часть slaughter-бонуса,
/// и обе стороны получают кулдаун (запись DuelResult с IsDecline).
/// </summary>
public static class DuelDecline
{
    /// <summary>Применяет отказ. Возвращает, сколько slaughter-бонуса ушло вызывающему.</summary>
    public static async Task<int> ApplyAsync(UserContext context, DuelRequest request, DateTime utcNow, string reason)
    {
        var attackerSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == request.AttackerId);
        var defenderSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == request.DefenderId);

        var defenderSlaughterWeight = await context.GetSlaughterWeightAsync(defenderSwine.OwnerId, defenderSwine.GroupId);
        var share = DuelResolution.GetDeclineSlaughterShareKg(defenderSlaughterWeight);

        if (share > 0 && defenderSwine.GroupId is { } groupId)
        {
            context.SlaughterBonuses.Add(new SlaughterBonus
            {
                UserId = defenderSwine.OwnerId,
                GroupId = groupId,
                Weight = -share,
                DateTime = utcNow,
                Reason = reason,
            });

            context.SlaughterBonuses.Add(new SlaughterBonus
            {
                UserId = attackerSwine.OwnerId,
                GroupId = groupId,
                Weight = share,
                DateTime = utcNow,
                Reason = reason,
            });
        }

        context.DuelResults.Add(new DuelResult
        {
            AttackerId = attackerSwine.SwineId,
            DefenderId = defenderSwine.SwineId,
            AttackerWon = true,
            IsDecline = true,
            DateTime = utcNow,
            WinnerWeightBefore = attackerSwine.Weight,
            WinnerWeightAfter = attackerSwine.Weight,
            LoserWeightBefore = defenderSwine.Weight,
            LoserWeightAfter = defenderSwine.Weight,
        });

        context.DuelRequests.Remove(request);
        return share;
    }
}

/// <summary>
/// Игнор вызова дольше TTL считается отказом. Вызывается лениво — когда бот вообще
/// что-то обрабатывает, отдельного планировщика нет.
/// </summary>
public static class DuelExpiry
{
    public static async Task<int> ApplyOverdueAsync(UserContext context, DateTime utcNow)
    {
        var overdue = await context.DuelRequests
            .AsTracking()
            .Where(dr => dr.DateTime < utcNow - DuelRequestExtensions.TTL)
            .ToListAsync();

        var applied = 0;
        foreach (var request in overdue)
        {
            await DuelDecline.ApplyAsync(context, request, utcNow, "ignored duel request");
            ++applied;
        }

        return applied;
    }
}
