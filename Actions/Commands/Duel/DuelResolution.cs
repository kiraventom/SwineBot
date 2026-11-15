namespace SwineBot.Actions.Commands.Duel;

/// <summary>
/// Итог состоявшейся дуэли: кто победил и что стало с весами.
/// </summary>
public record DuelOutcome(bool AttackerWon, int WinnerWeightBefore, int WinnerWeightAfter, int LoserWeightBefore, int LoserWeightAfter)
{
    public int StakeKg => WinnerWeightAfter - WinnerWeightBefore;
}

/// <summary>
/// Правила дуэли. Всё здесь — чистые функции: ни БД, ни рандома внутри,
/// поэтому их можно проверять харнессом с любым броском.
/// </summary>
public static class DuelResolution
{
    /// <summary>
    /// Шанс победы защитника, %: вес решает, скрытых статов нет.
    /// </summary>
    public static int GetDefenderWinChance(int attackerWeight, int defenderWeight)
        => (int)Math.Round((double)defenderWeight / (attackerWeight + defenderWeight) * 100);

    /// <summary>
    /// Ставка: доля веса проигравшего, но не меньше <see cref="DuelBalance.MinStakeKg"/>
    /// и так, чтобы проигравший не упал ниже <see cref="DuelBalance.MinWeightKg"/>.
    /// </summary>
    public static int GetStakeKg(int loserWeight)
    {
        var stake = Math.Max(DuelBalance.MinStakeKg, (int)Math.Round(loserWeight * DuelBalance.StakeShare));
        return Math.Min(stake, Math.Max(0, loserWeight - DuelBalance.MinWeightKg));
    }

    /// <summary>
    /// Штраф за отказ, % от веса защитника: базовая ставка плюс надбавка фавориту.
    /// Уклониться андердогу дешевле, фавориту — дорого, но бесплатно не бывает никому.
    /// </summary>
    public static int GetDeclineFeePercent(int defenderWinChance)
    {
        var favoritePremium = Math.Max(0, defenderWinChance - 50) * DuelBalance.DeclineFavoriteRate;
        var percent = DuelBalance.DeclineBasePercent + favoritePremium;
        return (int)Math.Round(Math.Min(percent, DuelBalance.MaxDeclinePercent));
    }

    /// <summary>Тот же штраф, но в килограммах, с учётом минимального веса.</summary>
    public static int GetDeclineFeeKg(int defenderWeight, int defenderWinChance)
    {
        var percent = GetDeclineFeePercent(defenderWinChance);
        var fee = (int)Math.Round(defenderWeight * percent / 100.0);

        var maxFee = Math.Max(0, defenderWeight - DuelBalance.MinWeightKg);
        return Math.Min(Math.Max(fee, 1), maxFee);
    }

    /// <summary>
    /// Бросок боя. <paramref name="roll"/> — равномерное [0, 1); защитник побеждает,
    /// если бросок попал в его шанс.
    /// </summary>
    public static DuelOutcome Resolve(int attackerWeight, int defenderWeight, double roll)
    {
        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight);
        var attackerWon = roll * 100 >= defenderWinChance;

        var winnerWeight = attackerWon ? attackerWeight : defenderWeight;
        var loserWeight = attackerWon ? defenderWeight : attackerWeight;
        var stake = GetStakeKg(loserWeight);

        return new DuelOutcome(attackerWon, winnerWeight, winnerWeight + stake, loserWeight, loserWeight - stake);
    }
}
