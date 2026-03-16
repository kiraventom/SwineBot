using Serilog;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class FeedManager
{
    public const int OVERFEED_COOLDOWN = 24;

    public Swine Swine { get; }
    public IReadOnlyCollection<IAchievementEffect> Effects { get; }
    public DateTime UtcNow { get; }
    public ThrowupCalculator ThrowupCalculator { get; }

    public FeedManager(Swine swine, IReadOnlyCollection<IAchievementEffect> effects)
    {
        Swine = swine;
        Effects = effects;
        UtcNow = DateTime.Now.ToUniversalTime();
        ThrowupCalculator = new ThrowupCalculator(UtcNow, Effects);
    }

    public FeedResult Generate()
    {
        var recentFeeds = Swine.Feeds.Where(f => (UtcNow - f.DateTime).TotalHours < OVERFEED_COOLDOWN).ToList();

        Result result = RollResult(recentFeeds);
        if (result == Result.Full)
            return FeedResult.Full;

        double luck = RollLuck();
        int absAmount = RollAmount(luck);
        int amount = ApplyResult(recentFeeds, absAmount, result);
        Log.Logger.Information("Final amount: {amount}", amount);

        return new FeedResult()
        {
            Luck = luck,
            Amount = amount,
            Result = result,
            OldWeight = Swine.Weight,
            RecentFeedsCount = recentFeeds.Count + 1,
            UtcDT = UtcNow
        };
    }

    private double RollLuck()
    {
        var luck = Random.Shared.NextDouble();
        Log.Logger.Information("Luck rolled: {luck}", luck);
        return luck;
    }

    private int RollAmount(double luck)
    {
        const int MAX_AMOUNT = 20;
        var baseAmount = (int)(MAX_AMOUNT * luck);
        Log.Logger.Information("Base amount rolled: {baseAmount}", baseAmount);
        var amount = ApplyEffects(baseAmount);
        Log.Logger.Information("Amount with effects: {amount}", amount);
        return amount;
    }

    private Result RollResult(IReadOnlyCollection<Feed> recentFeeds)
    {
        const int THROWUP_COOLDOWN = 24;

        var recentThrowups = Swine.WeightLosses
            .Where(wl => (UtcNow - wl.DateTime).TotalHours < THROWUP_COOLDOWN)
            .Where(wl => wl.IsThrowUp);

        if (recentThrowups.Any())
            return Result.Full;

        if (recentFeeds.Any() == false)
            return Result.FirstFeed;

        return ThrowupCalculator.IsThrowup(recentFeeds) ? Result.Throwup : Result.Overfeed;
    }

    private int ApplyEffects(int amount)
    {
        var totalSlaughteredWeight = Swine.Owner.Slaughters
            .Where(s => s.GroupId == Swine.GroupId)
            .Where(s => s.SwineWeight >= SlaughterMessage.MIN_SWINE_WEIGHT)
            .Sum(s => s.SwineWeight);

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);
        return (int)(amount * growthMod);
    }

    private int ApplyResult(IReadOnlyCollection<Feed> recentFeeds, int amount, Result result)
    {
        if (result == Result.Throwup)
            return ThrowupCalculator.Calculate(recentFeeds, Swine.Weight, amount) * -1;

        return amount;
    }
}

