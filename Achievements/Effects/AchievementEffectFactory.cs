namespace SwineBot.Achievements.Effects;

public class AchievementEffectFactory
{
    public IAchievementEffect Build(AchievementEffectType type, double value)
    {
        return type switch
        {
            AchievementEffectType.OverfeedScale => new OverfeedScaleModifierEffect(value),
            AchievementEffectType.ThrowupScale => new ThrowupScaleEffect(value),
            AchievementEffectType.ThrowupIgnoreChance => new ThrowupIgnoreChanceEffect(value),
            AchievementEffectType.LuckAmplify => new NoOverfeedsLuckAmplifierEffect(value),
            AchievementEffectType.NonClampedLuckAmplify => new NonClampedNoOverfeedsLuckAmplifierEffect(value),
            AchievementEffectType.MinLuck => new MinLuckEffect(value),
            _ => throw new NotSupportedException($"Unknown effect type \"{type}\"")
        };
    }
}

