using SwineBot.Achievements.Checkers;
using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public class NoOverfeedsLuckAmplifierEffect : DynamicAchievementEffect<double>
{
    private const double LUCK_AMPLIFY_MODIFIER = 0.02;
    public override string Description { get; } = $"Свин становится на {(int)Math.Round(LUCK_AMPLIFY_MODIFIER * 100)}% удачливее за каждый день без перекорма подряд";

    public override AchievementEffectType Type => AchievementEffectType.LuckAmplify;

    public override double Apply(UserContext context, int swineId, double value)
    {
        var consecutiveNoOverfeeds = NoOverfeedAchievementChecker.CountConsecutiveNoOverfeeds(context, swineId);
        var luckAmplify = LUCK_AMPLIFY_MODIFIER * consecutiveNoOverfeeds;
        value = value + value * luckAmplify;

        return Math.Clamp(value, 0.0, 1.0);
    }
}

