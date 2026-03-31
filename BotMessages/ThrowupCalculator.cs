using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public interface IThrowupCalculatorFactory
{
    IThrowupCalculator Create(DateTime utcNow, IReadOnlyCollection<IAchievementEffect> effects);
}

public class ThrowupCalculatorFactory(IServiceProvider sp) : IThrowupCalculatorFactory
{
    public IThrowupCalculator Create(DateTime utcNow, IReadOnlyCollection<IAchievementEffect> effects) => ActivatorUtilities.CreateInstance<ThrowupCalculator>(sp, utcNow, effects);
}

public interface IThrowupCalculator
{
    bool IsThrowup(IReadOnlyCollection<Feed> recentFeeds);
    int Calculate(IReadOnlyCollection<Feed> recentFeeds, int oldWeight, int amount);
}

public class ThrowupCalculator(ILogger<ThrowupCalculator> Logger, DateTime UtcNow, IReadOnlyCollection<IAchievementEffect> Effects) : IThrowupCalculator
{
    public bool IsThrowup(IReadOnlyCollection<Feed> recentFeeds)
    {
        const double OVERFEED_THROWUP_BASE_CHANCE = 0.01;

        var overfeedScale = GetOverfeedScale(recentFeeds);
        var throwupThreshold = OVERFEED_THROWUP_BASE_CHANCE * Math.Pow(overfeedScale, recentFeeds.Count);
        Logger.LogInformation("Throwup threshold: {baseChance} * {scale} ^ {recentFeedsCount} = {threshold}", OVERFEED_THROWUP_BASE_CHANCE, overfeedScale, recentFeeds.Count, throwupThreshold);

        var throwupThresholdBeforeClamping = throwupThreshold;
        throwupThreshold = Math.Min(0.99, throwupThreshold);

        if (throwupThreshold != throwupThresholdBeforeClamping)
            Logger.LogInformation("Throwup threshold: adjusted to being not 100% from {old} to {new}", throwupThresholdBeforeClamping, throwupThreshold);

        var overfeedChance = Random.Shared.NextDouble();
        var isThrowup = overfeedChance < throwupThreshold;
        if (isThrowup)
            Logger.LogInformation("Throwup: {overfeed} < {throwup}", overfeedChance, throwupThreshold);
        else
            Logger.LogInformation("No overfeed: {overfeed} >= {throwup}", overfeedChance, throwupThreshold);

        return isThrowup;
    }

    public int Calculate(IReadOnlyCollection<Feed> recentFeeds, int oldWeight, int amount)
    {
        int sum = recentFeeds.Sum(f => f.Amount);
        int amountLost = Math.Min(oldWeight - 1, sum + amount);

        Logger.LogInformation("Recent feeds: {recentFeeds} = {sum}; {sum} + {amount} = {amountLost}", string.Join(" + ", recentFeeds.Select(f => f.Amount)), sum, sum, amount, amountLost);

        foreach (var effect in Effects.OfType<ThrowupScaleEffect>())
        {
            var oldAmountLost = amountLost;
            amountLost = effect.Apply(amountLost);
            if (amountLost != oldAmountLost)
                Logger.LogInformation("Applied effect {effect}, amount lost changed from {old} to {new}", effect.Type.ToString(), oldAmountLost, amountLost);
        }

        foreach (var effect in Effects.OfType<ThrowupIgnoreChanceEffect>())
        {
            var oldAmountLost = amountLost;
            amountLost = effect.Apply(amountLost);
            if (amountLost != oldAmountLost)
                Logger.LogInformation("Applied effect {effect}, amount lost changed from {old} to {new}", effect.Type.ToString(), oldAmountLost, amountLost);
        }

        return amountLost;
    }

    private double GetOverfeedScale(IReadOnlyCollection<Feed> recentFeeds)
    {
        const double BASE_OVERFEED_SCALE = 2.5;

        const int OVERFEED_FADEOUT_HOURS = 12;
        const double OVERFEED_FADEOUT_SCALE = 0.75;
        const double FADED_OUT_OVERFEED_SCALE = BASE_OVERFEED_SCALE * OVERFEED_FADEOUT_SCALE;

        double overfeedScale = BASE_OVERFEED_SCALE;

        // Шанс на блёв максимальный (BASE_OVERFEED_SCALE) сразу после кормления, 
        // но постепенно уменьшается до BASE_OVERFEED_SCALE * OVERFEED_FADEOUT_SCALE следующие OVERFEED_FADEOUT_HOURS часов
        var lastFeed = recentFeeds.OrderBy(f => f.DateTime).LastOrDefault();
        if (lastFeed is not null)
        {
            double hoursSinceLastFeed = (UtcNow - lastFeed.DateTime).TotalHours;
            var clampedHours = Math.Clamp(hoursSinceLastFeed, 0, OVERFEED_FADEOUT_HOURS);
            var hoursScale = clampedHours / OVERFEED_FADEOUT_HOURS;
            overfeedScale = BASE_OVERFEED_SCALE - (BASE_OVERFEED_SCALE - FADED_OUT_OVERFEED_SCALE) * hoursScale;

            if (overfeedScale != BASE_OVERFEED_SCALE)
                Logger.LogInformation("Fadeout: Overfeed scale changed from {base} to {new}", BASE_OVERFEED_SCALE, overfeedScale);
        }

        foreach (var effect in Effects.OfType<OverfeedScaleModifierEffect>())
        {
            var oldScale = overfeedScale;
            overfeedScale = effect.Apply(overfeedScale);
            if (oldScale != overfeedScale)
                Logger.LogInformation("Applied effect {effect}, overfeed scale changed from {old} to {new}", effect.Type.ToString(), oldScale, overfeedScale);
        }

        return overfeedScale;
    }
}

