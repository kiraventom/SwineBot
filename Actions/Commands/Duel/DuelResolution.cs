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
    /// Шанс победы защитника, %: вес решает, но каждый забитый предшественник любой
    /// из сторон режет её шанс на <see cref="DuelBalance.KillPenaltyPoints"/> п.п.
    /// Серийного убийцу можно контрить, вызвав его на дуэль малышом.
    /// </summary>
    public static int GetDefenderWinChance(int attackerWeight, int defenderWeight, int attackerKills = 0, int defenderKills = 0)
    {
        var weightShare = (double)defenderWeight / (attackerWeight + defenderWeight) * 100;

        var chance = weightShare
                     - DuelBalance.KillPenaltyPoints * defenderKills
                     + DuelBalance.KillPenaltyPoints * attackerKills;

        return (int)Math.Round(Math.Clamp(chance, DuelBalance.MinWinChance, DuelBalance.MaxWinChance));
    }

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
    /// Матожидание изменения веса защитника за бой при текущем шансе. При нулевом
    /// дебаффе за забои оно равно нулю — поэтому и весовой штраф за отказ нулевой.
    /// </summary>
    public static double GetExpectedDelta(int attackerWeight, int defenderWeight, int attackerKills = 0, int defenderKills = 0)
    {
        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight, attackerKills, defenderKills) / 100.0;
        var win = defenderWinChance * GetStakeKg(attackerWeight);
        var lose = (1 - defenderWinChance) * GetStakeKg(defenderWeight);
        return win - lose;
    }

    /// <summary>
    /// Весовая часть штрафа за отказ, кг (обычно ноль: платят slaughter-бонусом).
    /// </summary>
    public static int GetDeclineFeeKg(int attackerWeight, int defenderWeight)
    {
        if (DuelBalance.DeclineRiskFactor <= 0)
            return DuelBalance.MinDeclineFeeKg;

        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight);
        var stake = GetStakeKg(defenderWeight);
        var loseChance = (100 - defenderWinChance) / 100.0;

        var fee = (int)Math.Round(stake * loseChance * DuelBalance.DeclineRiskFactor);
        var maxFee = Math.Max(0, defenderWeight - DuelBalance.MinWeightKg);
        return Math.Min(Math.Max(fee, DuelBalance.MinDeclineFeeKg), maxFee);
    }

    /// <summary>
    /// Сколько slaughter-бонуса защитник отдаёт вызывающему за отказ/игнор/забой.
    /// </summary>
    public static int GetDeclineSlaughterShareKg(int defenderSlaughterWeight)
        => defenderSlaughterWeight <= 0
            ? 0
            : (int)Math.Round(defenderSlaughterWeight * DuelBalance.DeclineSlaughterShare);

    /// <summary>
    /// Бросок боя. <paramref name="roll"/> — равномерное [0, 1); защитник побеждает,
    /// если бросок попал в его шанс.
    /// </summary>
    public static DuelOutcome Resolve(int attackerWeight, int defenderWeight, double roll, int attackerKills = 0, int defenderKills = 0)
    {
        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight, attackerKills, defenderKills);
        var attackerWon = roll * 100 >= defenderWinChance;

        var winnerWeight = attackerWon ? attackerWeight : defenderWeight;
        var loserWeight = attackerWon ? defenderWeight : attackerWeight;
        var stake = GetStakeKg(loserWeight);

        return new DuelOutcome(attackerWon, winnerWeight, winnerWeight + stake, loserWeight, loserWeight - stake);
    }
}
