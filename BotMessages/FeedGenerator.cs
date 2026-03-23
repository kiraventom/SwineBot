using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public interface IFeedGeneratorFactory
{
    IFeedGenerator Create(int swineId);
}

public class FeedGeneratorFactory(IServiceProvider sp) : IFeedGeneratorFactory
{
    public IFeedGenerator Create(int swineId) => ActivatorUtilities.CreateInstance<FeedGenerator>(sp, swineId);
}

public interface IFeedGenerator
{
    Swine Swine { get; }
    FeedResult Generate();
}

public class FeedGenerator : IFeedGenerator
{
    public const int OVERFEED_COOLDOWN = 24;

    private ILogger<FeedGenerator> Logger { get; }
    private UserContext Context { get; }
    private IAchievementController AchievController { get; }
    private IThrowupCalculator ThrowupCalculator { get; }

    public Swine Swine { get; }
    public IReadOnlyCollection<IAchievementEffect> Effects { get; }
    public DateTime UtcNow { get; }

    public FeedGenerator(ILogger<FeedGenerator> logger, UserContext context, IAchievementController achievController, IThrowupCalculatorFactory throwupCalcFactory, int SwineId)
    {
        Logger = logger;
        Context = context;
        AchievController = achievController;

        Swine = context.Swines
            .Include(s => s.Owner).ThenInclude(u => u.Slaughters)
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .Include(s => s.Info).ThenInclude(s => s.Achievements)
            .First(s => s.SwineId == SwineId);

        Effects = Swine.Info.Achievements
            .Select(a => AchievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToList();

        UtcNow = DateTime.Now.ToUniversalTime();
        ThrowupCalculator = throwupCalcFactory.Create(UtcNow, Effects);
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
        Logger.LogInformation("Final amount: {amount}", amount);

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
        Logger.LogInformation("Luck rolled: {luck}", baseLuck);

        var luck = ApplyLuckEffects(baseLuck);
        if (luck != baseLuck)
            Logger.LogInformation("Luck changed from {base} to {new}", baseLuck, luck);

        return luck;
    }

    private double ApplyLuckEffects(double luck)
    {
        foreach (var effect in Effects.OfType<NoOverfeedsLuckAmplifierEffect>())
            luck = effect.Apply(Context, Swine.SwineId, luck);

        return luck;
    }

    private int RollAmount(double luck)
    {
        const int MAX_AMOUNT = 20;
        var baseAmount = (int)Math.Round(MAX_AMOUNT * luck);
        baseAmount = Math.Max(1, baseAmount);
        Logger.LogInformation("Base amount rolled: {baseAmount}", baseAmount);
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
        Logger.LogInformation("Amount with effects: {amountEff}", amountEff);
        return (int)amountEff;
    }

    private int ApplyResult(IReadOnlyCollection<Feed> recentFeeds, int amount, Result result)
    {
        if (result == Result.Throwup)
            return ThrowupCalculator.Calculate(recentFeeds, Swine.Weight, amount) * -1;

        return amount;
    }
}

