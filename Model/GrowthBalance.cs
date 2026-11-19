namespace SwineBot.Model;

/// <summary>
/// Числа роста свина от забоёв.
///
/// Множитель — это не вечный пассивный доход, а разгон: он копится в «запасе»
/// (<see cref="Swine.MultiplierFuel"/>), пока свин лёгкий, и тратится по шагу за каждую
/// кормёжку, когда свин забрался выше <see cref="EliteThresholdShare"/> от веса лидера
/// беседы. Так он помогает вырваться с низов и не даёт бесконечно ехать на старых забоях.
///
/// Бонус к порции суб-линейный (корень), иначе убийца уходил в неубиваемый отрыв:
/// при Σ = 17 000 кг линейная формула давала 4.41 против 1.14 у бережливого.
/// </summary>
public static class GrowthBalance
{
    /// <summary>Растёт ли бонус суб-линейно (корень) или линейно (старое поведение).</summary>
    public static bool SubLinear { get; set; } = true;

    /// <summary>Бонус за корень из забитых килограммов (суб-линейная модель).</summary>
    public static double PerSqrtKg { get; set; } = 0.010;

    /// <summary>Бонус за килограмм (линейная модель, оставлена для сравнения).</summary>
    public const double PerKg = 0.0002;

    /// <summary>Ниже этого множитель не опускается: базовый рост есть у всех.</summary>
    public const double MinMultiplier = 1.0;

    /// <summary>
    /// Доля веса лидера беседы, с которой начинается трата запаса: 0.75 — три четверти
    /// веса самого толстого свина. Ниже порога множитель не тратится вообще.
    /// </summary>
    public static double EliteThresholdShare { get; set; } = 0.75;

    /// <summary>
    /// Насколько уменьшается множитель за каждую кормёжку выше порога — **доля излишка**
    /// над 1.0, а не абсолютная величина.
    ///
    /// Почему именно доля: приход от забоя в единицах множителя — всего ~0.002–0.003
    /// в сутки (забой раз в месяц по 1 000–1 500 кг даёт +0.06…0.11 к множителю). Любой
    /// фиксированный шаг больше этого прихода (а «4.0 → 3.8 за кормёжку» больше в 60 раз)
    /// прибивает множитель к 1.0 у всех, кто выше порога, и забой теряет смысл.
    ///
    /// При доле p равновесие считается так: `излишек = приход_в_сутки / (p · кормёжек_в_сутки)`,
    /// т.е. вершина садится на предсказуемый уровень: p = 0.004 → ~1.4, p = 0.01 → ~1.17.
    ///
    /// Порог 0.75 подобран прогонами: при 0.5 разгон сгорает ровно там, где идёт борьба за
    /// первое место (разрыв +5 904 кг), при 0.75 — баланс лучше всего (+1 605 при sd 3 730),
    /// при 0.9 лидер меняется чаще, но убийца слабеет (+3 114).
    /// </summary>
    public static double MultiplierDecayShare { get; set; } = 0.004;

    /// <summary>Множитель, который даёт накопленный запас.</summary>
    public static double ModifierFor(double fuel)
    {
        var clamped = Math.Max(0, fuel);
        var bonus = SubLinear ? PerSqrtKg * Math.Sqrt(clamped) : PerKg * clamped;
        return 1 + bonus;
    }

    /// <summary>Обратная операция: сколько запаса соответствует множителю.</summary>
    public static double FuelForModifier(double modifier)
    {
        var bonus = Math.Max(0, modifier - 1);
        return SubLinear ? Math.Pow(bonus / PerSqrtKg, 2) : bonus / PerKg;
    }

    /// <summary>Множитель после одной кормёжки выше порога: тратится доля излишка над 1.0.</summary>
    public static double DecayedModifier(double modifier)
    {
        if (modifier <= MinMultiplier)
            return MinMultiplier;

        var excess = modifier - MinMultiplier;
        var left = excess * (1 - Math.Clamp(MultiplierDecayShare, 0, 1));
        return MinMultiplier + left;
    }
}
