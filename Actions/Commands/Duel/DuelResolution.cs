namespace SwineBot.Actions.Commands.Duel;

/// <summary>Боевые характеристики свина для расчёта шанса победы.</summary>
public record DuelCombatStats(int AgeDays, int WitnessedKills, int AchievementPoints, int Glory)
{
    public static readonly DuelCombatStats Empty = new(0, 0, 0, 0);
}

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
///
/// v3: шанс — сжатая доля веса плюс слава; ставка привязана к более лёгкому свину,
/// поэтому дуэль не может решить год одним броском.
/// </summary>
public static class DuelResolution
{
    /// <summary>
    /// Боевой бонус свина к шансу победы, п.п. Источник задаётся настройкой
    /// <see cref="DuelBalance.ChanceSource"/>; в v3 по умолчанию выключен — работает слава.
    /// </summary>
    public static int GetCombatBonus(DuelCombatStats stats)
    {
        var raw = DuelBalance.ChanceSource switch
        {
            DuelChanceSource.Age => stats.AgeDays * DuelBalance.AgeBonusPerDay,
            DuelChanceSource.WitnessedKills => stats.WitnessedKills * DuelBalance.WitnessBonusPerKill,
            DuelChanceSource.Achievements => stats.AchievementPoints * DuelBalance.AchievementBonusPerPoint,
            _ => 0,
        };

        var capped = Math.Min(raw, DuelBalance.CombatBonusCap);
        return (int)Math.Round(capped);
    }

    /// <summary>
    /// Шанс победы защитника, %.
    ///
    /// Масса берётся не «как есть», а сжатой к 50%: при весах 1000 против 9000 защитник
    /// получает не 90%, а 74% — иначе у более лёгкого нет шансов и дуэль превращается
    /// в налог на слабого. Слава добавляет по <see cref="DuelBalance.GloryBonusPerPoint"/>
    /// за очко разницы: опытный свин побеждает чаще, но не гарантированно.
    /// </summary>
    public static int GetDefenderWinChance(int attackerWeight, int defenderWeight,
        DuelCombatStats attacker = null, DuelCombatStats defender = null)
    {
        var weightShare = (double)defenderWeight / Math.Max(1, attackerWeight + defenderWeight) * 100;

        var compressed = 50 + (weightShare - 50) * DuelBalance.WeightShareCompression;

        var attackerBonus = GetCombatBonus(attacker ?? DuelCombatStats.Empty);
        var defenderBonus = GetCombatBonus(defender ?? DuelCombatStats.Empty);

        var gloryDiff = (defender?.Glory ?? 0) - (attacker?.Glory ?? 0);
        var gloryBonus = DuelBalance.GloryBonusPerPoint * gloryDiff;

        // Инициатива у атакующего: он выбирает момент, поэтому шанс защитника ниже
        var chance = compressed - DuelBalance.AttackerInitiativeBonus
                     + defenderBonus - attackerBonus + gloryBonus;
        return (int)Math.Round(Math.Clamp(chance, DuelBalance.MinWinChance, DuelBalance.MaxWinChance));
    }

    /// <summary>
    /// Ставка в кг: доля веса **более лёгкого** свина. Так лидер, которому бросил вызов
    /// малыш, рискует малой долей своей туши, а ставка сама масштабируется с экономикой —
    /// абсолютные потолки не нужны.
    /// </summary>
    public static int GetStakeKg(int attackerWeight, int defenderWeight)
    {
        var lighter = Math.Min(attackerWeight, defenderWeight);
        var stake = Math.Max(DuelBalance.MinStakeKg, (int)Math.Round(lighter * DuelBalance.StakeShare));
        return stake;
    }

    /// <summary>
    /// Во сколько раз растёт забранная масса за славу победителя (+5% за очко, кап +25%).
    /// </summary>
    public static double GetGloryStakeMultiplier(int winnerGlory)
    {
        var bonus = Math.Min(DuelBalance.GloryStakeBonusCap,
            Math.Max(0, winnerGlory) * DuelBalance.GloryStakeBonusPerPoint);
        return 1 + bonus;
    }

    /// <summary>Слава победителя за бой: за победу и за апсет, если бил не легче вдвое.</summary>
    public static int GetGloryReward(bool attackerWon, int winnerWeightBefore, int loserWeightBefore)
    {
        if (!DuelBalance.GloryEnabled)
            return 0;

        var glory = DuelBalance.GloryPerWin;

        var upset = winnerWeightBefore <= loserWeightBefore / DuelBalance.UpsetWeightRatio;
        if (upset)
            glory += DuelBalance.GloryPerUpset;

        return glory;
    }

    /// <summary>
    /// Весовая часть штрафа за отказ, кг (в v3 всегда ноль: отказ платится славой).
    /// </summary>
    public static int GetDeclineFeeKg(int attackerWeight, int defenderWeight)
    {
        if (DuelBalance.DeclineRiskFactor <= 0)
            return DuelBalance.MinDeclineFeeKg;

        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight);
        var stake = GetStakeKg(attackerWeight, defenderWeight);
        var loseChance = (100 - defenderWinChance) / 100.0;

        var fee = (int)Math.Round(stake * loseChance * DuelBalance.DeclineRiskFactor);
        var maxFee = Math.Max(0, defenderWeight - DuelBalance.MinWeightKg);
        return Math.Min(Math.Max(fee, DuelBalance.MinDeclineFeeKg), maxFee);
    }

    /// <summary>
    /// Бросок боя. <paramref name="roll"/> — равномерное [0, 1); защитник побеждает,
    /// если бросок попал в его шанс.
    /// </summary>
    public static DuelOutcome Resolve(int attackerWeight, int defenderWeight, double roll,
        DuelCombatStats attacker = null, DuelCombatStats defender = null)
    {
        var attackerStats = attacker ?? DuelCombatStats.Empty;
        var defenderStats = defender ?? DuelCombatStats.Empty;

        var defenderWinChance = GetDefenderWinChance(attackerWeight, defenderWeight, attackerStats, defenderStats);
        var attackerWon = roll * 100 >= defenderWinChance;

        var winnerWeight = attackerWon ? attackerWeight : defenderWeight;
        var loserWeight = attackerWon ? defenderWeight : attackerWeight;
        var winnerGlory = attackerWon ? attackerStats.Glory : defenderStats.Glory;

        // Ставка от лёгкого: проигравший отдаёт не больше 5% своей туши
        var stake = GetStakeKg(attackerWeight, defenderWeight);
        stake = (int)Math.Round(stake * GetGloryStakeMultiplier(winnerGlory));
        stake = Math.Min(stake, Math.Max(0, loserWeight - DuelBalance.MinWeightKg));

        return new DuelOutcome(attackerWon, winnerWeight, winnerWeight + stake, loserWeight, loserWeight - stake);
    }
}
