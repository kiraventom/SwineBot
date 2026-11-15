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

    /// <summary>Базовый штраф за отказ, % от веса защитника. Платится всегда.</summary>
    public const double DeclineBasePercent = 3;

    /// <summary>Надбавка за отказ фаворита: (шанс на победу − 50) × коэффициент, %.</summary>
    public const double DeclineFavoriteRate = 0.75;

    /// <summary>Потолок штрафа за отказ, % от веса защитника.</summary>
    public const int MaxDeclinePercent = 50;
}
