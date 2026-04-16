using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public enum AchievementEffectType
{
    None = 0,
    OverfeedScale = 1,
    ThrowupScale = 2,
    ThrowupIgnoreChance = 3,
    LuckAmplify = 4,
}

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
            _ => throw new NotSupportedException($"Unknown effect type \"{type}\"")
        };
    }
}

public interface IAchievementEffect
{
    string Description { get; }
    AchievementEffectType Type { get; }
}

public abstract class AchievementEffect : IAchievementEffect
{
    public abstract string Description { get; }
    public abstract AchievementEffectType Type { get; }
}

/// <summary>
/// Static effect, that works the same for every swine
/// </summary>
public abstract class StaticAchievementEffect<T> : AchievementEffect
{
    public abstract Task<T> Apply(T value);
}

/// <summary>
/// Dynamic effect, that works differently based on whom is it applied to
/// </summary>
public abstract class DynamicAchievementEffect<T> : AchievementEffect
{
    public abstract Task<T> Apply(UserContext context, int swineId, T value);
}
