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
    /// Штраф за отказ, кг: ожидаемая потеря защитника в бою, то есть
    /// ставка (сколько он теряет при поражении) × шанс этого поражения.
    /// Так отказ не выгоднее и не дороже боя, и никто не получает преимущества
    /// от выбранной политики «драться всегда» или «отказываться всегда».
    /// </summary>
    public static int GetDeclineFeeKg(int attackerWeight, int defenderWeight)
    {
        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight);
        var stake = GetStakeKg(defenderWeight);
        var loseChance = (100 - defenderWinChance) / 100.0;

        var fee = (int)Math.Round(stake * loseChance * DuelBalance.DeclineRiskFactor);

        var maxFee = Math.Max(0, defenderWeight - DuelBalance.MinWeightKg);
        return Math.Min(Math.Max(fee, DuelBalance.MinDeclineFeeKg), maxFee);
    }

    /// <summary>
    /// Матожидание изменения веса защитника за бой: p_победы × ставка атакующего
    /// минус p_поражения × своя ставка. При ставке «доля веса проигравшего» и шансе,
    /// пропорциональном весу, это ровно ноль — поэтому и штраф за отказ символический.
    /// </summary>
    public static double GetExpectedDelta(int attackerWeight, int defenderWeight)
    {
        // Шанс берём точный, а не округлённый до процентов: иначе на больших весах
        // округление даёт ложные «-2 кг» там, где по формуле ровно ноль
        var defenderWinChance = (double)defenderWeight / (attackerWeight + defenderWeight);
        var win = defenderWinChance * GetStakeKg(attackerWeight);
        var lose = (1 - defenderWinChance) * GetStakeKg(defenderWeight);
        return win - lose;
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
