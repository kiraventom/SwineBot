using SwineBot.Achievements.Checkers;
using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public class NoOverfeedsLuckAmplifierEffect(double luckAmplifyMod) : DynamicAchievementEffect<double>
{
    public override string Description => $"Свин ест на {(int)Math.Round(LuckAmplifyModifier * 100)}% больше за каждый день без перекорма подряд (не больше {MAX_LUCK * 100}%)";

    public override AchievementEffectType Type => AchievementEffectType.LuckAmplify;

    public const double MAX_LUCK = 2.0;

    public double LuckAmplifyModifier { get; } = luckAmplifyMod;

    public override async Task<double> Apply(UserContext context, int swineId, double value)
    {
        var consecutiveNoOverfeeds = await NoOverfeedChecker.CountConsecutiveNoOverfeeds(context, swineId);
        var luckAmplify = LuckAmplifyModifier * consecutiveNoOverfeeds;
        value = value + value * luckAmplify;

        return Math.Clamp(value, 0.0, MAX_LUCK);
    }
}
