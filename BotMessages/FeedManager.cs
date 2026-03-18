using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Achievements;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class FeedManager
{
    public const int OVERFEED_COOLDOWN = 24;
    private readonly UserContext _userContext;

    public Swine Swine { get; }
    public IReadOnlyCollection<IAchievementEffect> Effects { get; }
    public DateTime UtcNow { get; }
    public ThrowupCalculator ThrowupCalculator { get; }

    public FeedManager(UserContext userContext, int swineId, AchievementController achievController)
    {
        _userContext = userContext;
        Swine = userContext.Swines
            .Include(s => s.Owner).ThenInclude(u => u.Slaughters)
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
            .First(s => s.SwineId == swineId);
;
        Effects = Swine.Stats.Achievements
            .Select(a => achievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToList();

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
        var baseLuck = Random.Shared.NextDouble();
        Log.Logger.Information("Luck rolled: {luck}", baseLuck);

        var luck = ApplyLuckEffects(baseLuck);
        if (luck != baseLuck)
            Log.Logger.Information("Luck changed from {base} to {new}", baseLuck, luck);

        return luck;
    }

    private double ApplyLuckEffects(double luck)
    {
        foreach (var effect in Effects.OfType<NoOverfeedsLuckAmplifierEffect>())
            luck = effect.Apply(_userContext, Swine.SwineId, luck);

        return luck;
    }

    private int RollAmount(double luck)
    {
        const int MAX_AMOUNT = 20;
        var baseAmount = (int)Math.Round(MAX_AMOUNT * luck);
        baseAmount = Math.Max(1, baseAmount);
        Log.Logger.Information("Base amount rolled: {baseAmount}", baseAmount);
        var amount = ApplyAmountEffects(baseAmount);
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

    private int ApplyAmountEffects(int amount)
    {
        var totalSlaughteredWeight = Swine.Owner.Slaughters
            .Where(s => s.GroupId == Swine.GroupId)
            .Where(s => s.SwineWeight >= SlaughterMessage.MIN_SWINE_WEIGHT)
            .Sum(s => s.SwineWeight);

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        var amountEff = Math.Round(amount * growthMod);
        Log.Logger.Information("Amount with effects: {amountEff}", amountEff);
        return (int)amountEff;
    }

    private int ApplyResult(IReadOnlyCollection<Feed> recentFeeds, int amount, Result result)
    {
        if (result == Result.Throwup)
            return ThrowupCalculator.Calculate(recentFeeds, Swine.Weight, amount) * -1;

        return amount;
    }
}

