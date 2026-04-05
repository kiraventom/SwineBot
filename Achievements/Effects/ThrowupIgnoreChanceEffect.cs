namespace SwineBot.Achievements.Effects;

public class ThrowupIgnoreChanceEffect : StaticAchievementEffect<int>
{
    public override string Description { get; }
    public override AchievementEffectType Type => AchievementEffectType.ThrowupChance;

    public double Chance { get; }

    public ThrowupIgnoreChanceEffect(double chance)
    {
        Chance = chance;

        Description =
            "С шансом"
            + $" {(int)Math.Round(Chance * 100)}% "
            + "свин не блеванёт при неудачном перекорме";
    }

    public override Task<int> Apply(int value)
    {
        return Task.FromResult(Random.Shared.NextDouble() < Chance ? 0 : value);
    }
}
