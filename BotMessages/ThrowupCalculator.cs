using Serilog;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class ThrowupCalculator(DateTime utcNow, IReadOnlyCollection<IAchievementEffect> effects)
{
    public bool IsThrowup(IReadOnlyCollection<Feed> recentFeeds)
    {
        const double OVERFEED_THROWUP_BASE_CHANCE = 0.01;

        var overfeedScale = GetOverfeedScale(recentFeeds);
        var throwupThreshold = OVERFEED_THROWUP_BASE_CHANCE * Math.Pow(overfeedScale, recentFeeds.Count);
        throwupThreshold = Math.Min(0.99, throwupThreshold);

        var overfeedChance = Random.Shared.NextDouble();
        var isThrowup = overfeedChance < throwupThreshold;
        if (isThrowup)
            Log.Logger.Information("Throwup: {overfeed} < {throwup}", overfeedChance, throwupThreshold);
        else
            Log.Logger.Information("No overfeed: {overfeed} >= {throwup}", overfeedChance, throwupThreshold);

        return isThrowup;
    }

    public int Calculate(IReadOnlyCollection<Feed> recentFeeds, int oldWeight, int amount)
    {
        int amountLost = Math.Min(oldWeight - 1, recentFeeds.Sum(f => f.Amount) + amount);
        int initialAmountLost = amountLost;

        foreach (var effect in effects.OfType<ThrowupScaleEffect>())
            amountLost = effect.Apply(amountLost);

        foreach (var effect in effects.OfType<ThrowupIgnoreChanceEffect>())
            amountLost = effect.Apply(amountLost);

        if (amountLost != initialAmountLost)
            Log.Logger.Information("Throwup changed from {base} to {new}", initialAmountLost, amountLost);

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
            double hoursSinceLastFeed = (utcNow - lastFeed.DateTime).TotalHours;
            var clampedHours = Math.Clamp(hoursSinceLastFeed, 0, OVERFEED_FADEOUT_HOURS);
            var hoursScale = clampedHours / OVERFEED_FADEOUT_HOURS;
            overfeedScale = BASE_OVERFEED_SCALE - (BASE_OVERFEED_SCALE - FADED_OUT_OVERFEED_SCALE) * hoursScale;

            if (overfeedScale != BASE_OVERFEED_SCALE)
                Log.Logger.Information("Fadeout: Overfeed scale changed from {base} to {new}", BASE_OVERFEED_SCALE, overfeedScale);
        }

        double initialOverfeedScale = overfeedScale;

        foreach (var effect in effects.OfType<OverfeedScaleModifierEffect>())
            overfeedScale = effect.Apply(overfeedScale);

        if (overfeedScale != initialOverfeedScale)
            Log.Logger.Information("Effects: Overfeed scale changed from {base} to {new}", initialOverfeedScale, overfeedScale);

        return overfeedScale;
    }
}

