using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public enum AchievementEffectType
{
    None = 0,
    OverfeedScale = 1
}

public interface IAchievementEffect
{
    string Description { get; }
}

public abstract class AchievementEffect<T> : IAchievementEffect
{
    public abstract string Description { get; }
    public abstract AchievementEffectType Type { get; }
    public abstract T Apply(T value);
}
