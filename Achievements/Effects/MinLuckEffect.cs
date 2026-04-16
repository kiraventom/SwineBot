using SwineBot.BotMessages.Feed;
using SwineBot.Text;

namespace SwineBot.Achievements.Effects;

public class MinLuckEffect : StaticAchievementEffect<double>
{
    public override string Description { get; }
    public override AchievementEffectType Type => AchievementEffectType.MinLuck;

    public double MinLuck { get; }

    public MinLuckEffect(double minLuck)
    {
        MinLuck = minLuck;

        var minAmount = (int)Math.Round(FeedGenerator.MAX_FEED_AMOUNT * MinLuck);
        Description =
            "Свин никогда не ест меньше "
            + minAmount.ToString()
            + $" {MessageTextUtils.GetDeclinatedNoun(minAmount, Unit.Kg)}";
    }

    public override Task<double> Apply(double value)
    {
        return Task.FromResult(Math.Max(value, MinLuck));
    }
}
