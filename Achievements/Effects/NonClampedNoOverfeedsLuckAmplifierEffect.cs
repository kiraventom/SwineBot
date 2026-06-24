using SwineBot.Achievements.Checkers;
using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public class NonClampedNoOverfeedsLuckAmplifierEffect(double luckAmplifyMod) : DynamicAchievementEffect<double>
{
    public override string Description => $"Свин ест на {(int)Math.Round(LuckAmplifyModifier * 100)}% больше за каждый день без перекорма подряд (без ограничений)";

    public override AchievementEffectType Type => AchievementEffectType.NonClampedLuckAmplify;

    public double LuckAmplifyModifier { get; } = luckAmplifyMod;

    public override async Task<double> Apply(UserContext context, int swineId, double value)
    {
        var consecutiveNoOverfeeds = await NoOverfeedChecker.CountConsecutiveNoOverfeeds(context, swineId);
        var luckAmplify = LuckAmplifyModifier * consecutiveNoOverfeeds;
        value = value + value * luckAmplify;

        return value;
    }
}

