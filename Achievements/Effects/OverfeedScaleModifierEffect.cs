namespace SwineBot.Achievements.Effects;

public class OverfeedScaleModifierEffect : AchievementEffect<double>
{
    public override string Description { get; } 
    public override AchievementEffectType Type => AchievementEffectType.OverfeedScale;

    public double Modifier { get; }

    public OverfeedScaleModifierEffect(double modifier)
    {
        Modifier = modifier;

        var mod = Modifier - 1.0;

        Description =
            "Шанс блёва при перекорме "
            + (mod < 0 ? "снижен" : "увеличен")
            + $" на {(int)Math.Round(Math.Abs(mod) * 100)}%";
    }

    public override double Apply(double value)
    {
        return value * Modifier;
    }
}

