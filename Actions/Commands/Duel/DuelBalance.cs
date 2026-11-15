namespace SwineBot.Actions.Commands.Duel;

/// <summary>
/// Все числа баланса дуэлей. Крутить здесь, больше нигде править не нужно.
/// </summary>
public static class DuelBalance
{
    /// <summary>
    /// Сколько часов свин отдыхает после дуэли. Действует на обе стороны и на отказ тоже,
    /// иначе одного толстого свина можно фармить вызовами бесконечно.
    /// Можно переопределить в config.json полем "DuelCooldownHours" (для тестов — 0).
    /// </summary>
    public const int CooldownHours = 12;

    /// <summary>Доля веса проигравшего, которая переходит победителю.</summary>
    public const double StakeShare = 0.2;

    /// <summary>Минимальная ставка в кг: даже у худого свина бой что-то стоит.</summary>
    public const int MinStakeKg = 1;

    /// <summary>Минимальный вес свина: проигравший не может упасть ниже.</summary>
    public const int MinWeightKg = 1;

    /// <summary>
    /// Доля slaughter-бонуса (веса забитых предшественников), которую защитник отдаёт
    /// вызывающему за отказ, игнор вызова или забой свина под вызовом. Лорно: бонус — это
    /// милость свиного бога за жертвоприношения, а трусов и палачей он не уважает.
    /// </summary>
    public const double DeclineSlaughterShare = 0.10;

    /// <summary>
    /// Сколько процентных пунктов шанса победы снимает каждый забитый предшественник.
    /// Свин чувствует, что с ним могут поступить так же, и дерётся хуже.
    /// </summary>
    public const double KillPenaltyPoints = 2.0;

    /// <summary>Нижняя граница шанса победы, %: шанс должен быть всегда.</summary>
    public const int MinWinChance = 5;

    /// <summary>Верхняя граница шанса победы, %: безоговорочных побед не бывает.</summary>
    public const int MaxWinChance = 95;

    /// <summary>
    /// Множитель весовой части штрафа за отказ (0 — только slaughter-бонус, как задумано).
    /// Оставлен для экспериментов: 1.0 = «отказ стоит столько же, сколько бой в среднем».
    /// </summary>
    public static readonly double DeclineRiskFactor = 0.0;

    /// <summary>Минимальный вес откупа за отказ, кг. 0 — вес за отказ не отбирается.</summary>
    public const int MinDeclineFeeKg = 0;
}
