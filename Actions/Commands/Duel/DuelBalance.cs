namespace SwineBot.Actions.Commands.Duel;

/// <summary>Откуда берётся боевой бонус к шансу победы.</summary>
public enum DuelChanceSource
{
    /// <summary>Без бонуса: только вес.</summary>
    None,

    /// <summary>За возраст свина в днях (с первого покорма).</summary>
    Age,

    /// <summary>За забои, пережитые в беседе за время жизни свина.</summary>
    WitnessedKills,

    /// <summary>За ачивки: сумма уровней.</summary>
    Achievements,
}

/// <summary>Чем платит свин за отказ от дуэли.</summary>
public enum DuelDebuff
{
    /// <summary>Ничем, кроме славы (в v3 — единственный вариант).</summary>
    None,

    /// <summary>Часть кормёжек в течение суток съедает вызывавший. Выключено в v3.</summary>
    FeedSteal,

    /// <summary>Ценность свина для забоя падает вдвое за каждый отказ. Выключено в v3.</summary>
    Depreciate,
}

/// <summary>
/// Все числа баланса дуэлей. Крутить здесь, больше нигде править не нужно.
///
/// Правила v3 (см. DUELS-V3-DESIGN.md): дуэль — про славу, масса ходит понемногу.
/// Ставка привязана к более лёгкому свину, поэтому один бой никогда не решает год,
/// а отказ стоит репутации, а не еды.
/// </summary>
public static class DuelBalance
{
    /// <summary>Куда смотрит бонус к шансу помимо славы. В v3 слава — основной источник.</summary>
    public static DuelChanceSource ChanceSource { get; set; } = DuelChanceSource.None;

    /// <summary>Чем платит защитник за отказ. В v3 — только славой (-1).</summary>
    public static DuelDebuff Debuff { get; set; } = DuelDebuff.None;

    /// <summary>Начисляются ли очки славы за победу и тратятся ли на отказ.</summary>
    public static bool GloryEnabled { get; set; } = true;

    /// <summary>
    /// Насколько сильно масса двигает шанс: 1.0 — как раньше (доля веса),
    /// 0.6 — «сжатие к 50%», чтобы апсет оставался возможным.
    /// </summary>
    public const double WeightShareCompression = 0.6;

    /// <summary>Бонус к шансу победы за очко славы, п.п.</summary>
    public const double GloryBonusPerPoint = 2.0;

    /// <summary>
    /// Сдвиг в пользу атакующего, п.п.: он выбирает время и место боя. Заодно это
    /// источник апсетов — без него более лёгкий побеждает слишком редко.
    /// </summary>
    public const double AttackerInitiativeBonus = 5.0;

    /// <summary>Доля веса более лёгкого свина, которая переходит победителю.</summary>
    public static double StakeShare { get; set; } = 0.05;

    /// <summary>Минимальная ставка в кг: даже у худого свина бой что-то стоит.</summary>
    public const int MinStakeKg = 1;

    /// <summary>Минимальный вес свина: проигравший не может упасть ниже.</summary>
    public const int MinWeightKg = 1;

    /// <summary>Бонус к шансу за каждый день жизни свина, п.п. (если ChanceSource = Age).</summary>
    public const double AgeBonusPerDay = 0.25;

    /// <summary>Бонус к шансу за каждый забой, пережитый при жизни свина, п.п.</summary>
    public const double WitnessBonusPerKill = 0.5;

    /// <summary>Бонус к шансу за очко ачивок (сумма уровней), п.п.</summary>
    public const double AchievementBonusPerPoint = 1.5;

    /// <summary>Потолок бонуса от источника шанса, п.п.</summary>
    public const int CombatBonusCap = 30;

    /// <summary>Слава за победу в дуэли.</summary>
    public const int GloryPerWin = 1;

    /// <summary>Дополнительная слава за апсет: победил того, кто был не легче вдвое.</summary>
    public const int GloryPerUpset = 1;

    /// <summary>Во сколько раз побеждённый должен быть тяжелее для апсета.</summary>
    public const double UpsetWeightRatio = 2.0;

    /// <summary>Слава, которая тратится на отказ от вызова (и на игнор до TTL).</summary>
    public const int GloryPerDecline = 1;

    /// <summary>Верхний предел славы.</summary>
    public const int MaxGlory = 5;

    /// <summary>Нижний предел славы.</summary>
    public const int MinGlory = -5;

    /// <summary>
    /// Раз в сколько часов слава затухает на единицу к нулю. 0 — не затухает:
    /// слава — длинная прогрессия, а не временный модификатор.
    /// </summary>
    public static int GloryDecayHours { get; set; } = 0;

    /// <summary>Бонус к добыче корма за очко славы (+3% за очко).</summary>
    public const double GloryLuckBonusPerPoint = 0.03;

    /// <summary>Потолок бонуса славы к кормёжке (+15%).</summary>
    public const double GloryLuckBonusCap = 0.15;

    /// <summary>Бонус к забранной массе за очко славы победителя (+5% за очко).</summary>
    public const double GloryStakeBonusPerPoint = 0.05;

    /// <summary>Потолок бонуса славы к забранной массе (+25%).</summary>
    public const double GloryStakeBonusCap = 0.25;

    /// <summary>Сколько часов действует дебафф за отказ (в v3 не используется).</summary>
    public const int DebuffHours = 24;

    /// <summary>База «шанса поесть» при страхе. В v3 не используется.</summary>
    public const double FearKeepChancePerDebt = 0.75;

    /// <summary>Сжигается ли долг на неудачной кормёжке. В v3 не используется.</summary>
    public const bool FearBurnsDebt = true;

    /// <summary>Скольким случайным активным свинам достаётся порция труса. В v3 не используется.</summary>
    public const int FearRecipients = 3;

    /// <summary>Свин считается активным, если ел за последние столько часов.</summary>
    public const int ActiveFeederHours = 48;

    /// <summary>Во сколько раз падает ценность свина для забоя за каждый отказ (выключено в v3).</summary>
    public const double DepreciateFactor = 1.0;

    /// <summary>
    /// Сколько часов беседа обходится без новых вызовов. Дуэль — событие, а не поток:
    /// один вызов на беседу в сутки.
    /// </summary>
    public const int CallCooldownHours = 24;

    /// <summary>Как часто свин может отправлять вызов, часы.</summary>
    public const int SendCooldownHours = 24;

    /// <summary>Как часто свина можно вызывать (в роли защитника), часы.</summary>
    public const int DefendCooldownHours = 48;

    /// <summary>Через сколько часов ту же пару (атакующий → защитник) можно вызвать снова.</summary>
    public const int PairCooldownHours = 72;

    /// <summary>Нижняя граница шанса победы, %.</summary>
    public static int MinWinChance { get; set; } = 10;

    /// <summary>Верхняя граница шанса победы, %.</summary>
    public static int MaxWinChance { get; set; } = 90;

    /// <summary>Весовая часть штрафа за отказ, кг (0 — отказ весом не платится).</summary>
    public static readonly double DeclineRiskFactor = 0.0;

    /// <summary>Минимальный вес откупа за отказ, кг.</summary>
    public const int MinDeclineFeeKg = 0;
}
