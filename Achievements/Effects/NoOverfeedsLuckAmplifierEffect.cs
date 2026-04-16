using SwineBot.Achievements.Checkers;
using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public class NoOverfeedsLuckAmplifierEffect(double luckAmplifyMod) : DynamicAchievementEffect<double>
{
    public override string Description => $"Свин становится на {(int)Math.Round(LuckAmplifyModifier * 100)}% удачливее за каждый день без перекорма подряд";

    public override AchievementEffectType Type => AchievementEffectType.LuckAmplify;

    public double LuckAmplifyModifier { get; } = luckAmplifyMod;

    public override async Task<double> Apply(UserContext context, int swineId, double value)
    {
        var consecutiveNoOverfeeds = await NoOverfeedAchievementChecker.CountConsecutiveNoOverfeeds(context, swineId);
        var luckAmplify = LuckAmplifyModifier * consecutiveNoOverfeeds;
        value = value + value * luckAmplify;

        return Math.Clamp(value, 0.0, 1.0);
    }
}

