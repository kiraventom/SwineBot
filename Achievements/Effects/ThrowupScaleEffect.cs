namespace SwineBot.Achievements.Effects;

public class ThrowupScaleEffect : AchievementEffect<int>
{
    public override string Description { get; }
    public override AchievementEffectType Type => AchievementEffectType.ThrowupScale;

    public double Modifier { get; }

    public ThrowupScaleEffect(double modifier)
    {
        Modifier = modifier;

        var mod = Modifier - 1.0;

        Description =
            "При неудачном перекорме теряется на"
            + $" {(int)Math.Round(Math.Abs(mod) * 100)}% "
            + (mod < 0 ? "меньше" : "больше")
            + " веса";
    }

    public override int Apply(int value)
    {
        return (int)(value * Modifier);
    }
}


