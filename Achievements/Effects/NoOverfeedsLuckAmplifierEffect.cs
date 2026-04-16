using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Effects;

public class NoOverfeedsLuckAmplifierEffect(double luckAmplifyMod) : DynamicAchievementEffect<double>
{
    // TODO GetDescription
    public override string Description => $"Свин ест на {(int)Math.Round(LuckAmplifyModifier * 100)}% больше за каждый день без перекорма подряд (максимум {FeedGenerator.MAX_FEED_AMOUNT} кг)";

    public override AchievementEffectType Type => AchievementEffectType.LuckAmplify;

    public double LuckAmplifyModifier { get; } = luckAmplifyMod;

    public override async Task<double> Apply(UserContext context, int swineId, double value)
    {
        var consecutiveNoOverfeeds = await NoOverfeedChecker.CountConsecutiveNoOverfeeds(context, swineId);
        var luckAmplify = LuckAmplifyModifier * consecutiveNoOverfeeds;
        value = value + value * luckAmplify;

        return Math.Clamp(value, 0.0, 1.0);
    }
}
