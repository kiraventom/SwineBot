using Microsoft.EntityFrameworkCore;
using SwineBot.Model;

namespace SwineBot.Actions.Commands.Duel;

/// <summary>Тексты для сообщений про отказ.</summary>
public static class DuelDeclineText
{
    public static string Describe(DuelDebuff debuff) => debuff switch
    {
        DuelDebuff.FeedSteal => "Отказ: свин может струсить, и порция уйдёт трём активным свинам",
        DuelDebuff.Depreciate => "Отказ: ценность свина для забоя падает вдвое",
        _ => "Отказ: −1 славы, и на труса можно напасть без очереди",
    };

    public static string DescribeApplied(DuelDebuff debuff) => debuff switch
    {
        DuelDebuff.FeedSteal => "Свиной бог не уважает трусов: теперь свин может струсить",
        DuelDebuff.Depreciate => "Свиной бог не уважает трусов: ценность свина для забоя упала вдвое",
        _ => "Свиной бог записал свина в трусы: −1 славы",
    };
}

/// <summary>
/// «Дуэль не состоялась»: отказ, игнор дольше TTL или забой свина под вызовом.
/// Защитник платит дебаффом по настройке <see cref="DuelBalance.Debuff"/> — если у него
/// есть очки славы, он может расплатиться ими и уйти от дебаффа.
/// </summary>
public static class DuelDecline
{
    /// <summary>Применяет отказ. Возвращает текст о том, чем именно заплатил защитник.</summary>
    public static async Task<string> ApplyAsync(UserContext context, DuelRequest request, DateTime utcNow, string reason)
    {
        var attackerSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == request.AttackerId);
        var defenderSwine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == request.DefenderId);

        // Отказ стоит очка славы и делает свина трусом: на него можно напасть без очереди
        if (DuelBalance.GloryEnabled)
            defenderSwine.Glory = Math.Clamp(defenderSwine.Glory - DuelBalance.GloryPerDecline,
                DuelBalance.MinGlory, DuelBalance.MaxGlory);

        switch (DuelBalance.Debuff)
        {
            case DuelDebuff.Depreciate:
                defenderSwine.Declines += 1;
                break;
        }

        var payment = defenderSwine.Glory < 0
            ? $"Слава в минусе ({defenderSwine.Glory}): теперь на свина можно напасть без очереди"
            : DuelDeclineText.DescribeApplied(DuelBalance.Debuff);

        context.DuelResults.Add(new DuelResult
        {
            AttackerId = attackerSwine.SwineId,
            DefenderId = defenderSwine.SwineId,
            GroupId = defenderSwine.GroupId,
            AttackerWon = true,
            IsDecline = true,
            DateTime = utcNow,
            WinnerWeightBefore = attackerSwine.Weight,
            WinnerWeightAfter = attackerSwine.Weight,
            LoserWeightBefore = defenderSwine.Weight,
            LoserWeightAfter = defenderSwine.Weight,
        });

        context.DuelRequests.Remove(request);
        return payment;
    }
}

/// <summary>
/// Игнор вызова дольше TTL считается отказом. Вызывается лениво — когда бот вообще
/// что-то обрабатывает, отдельного планировщика нет.
/// </summary>
public static class DuelExpiry
{
    /// <summary>
    /// Отвага затухает на единицу к нулю раз в сутки: нельзя накопить вечный иммунитет
    /// или вечный долг. Вызывается лениво, как и обработка просроченных вызовов.
    /// </summary>
    public static async Task<int> DecayGloryAsync(UserContext context, DateTime utcNow)
    {
        // Слава — длинная прогрессия: с GloryDecayHours = 0 она не затухает вообще
        if (DuelBalance.GloryDecayHours <= 0)
            return 0;

        var decayFrom = utcNow.AddHours(-DuelBalance.GloryDecayHours);

        var withGlory = await context.Swines
            .AsTracking()
            .Where(s => s.Glory != 0)
            .Where(s => s.GloryDecayedAt == null || s.GloryDecayedAt < decayFrom)
            .ToListAsync();

        foreach (var swine in withGlory)
        {
            swine.Glory -= Math.Sign(swine.Glory);
            swine.GloryDecayedAt = utcNow;
        }

        return withGlory.Count;
    }

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
