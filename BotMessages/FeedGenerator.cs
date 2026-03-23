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

    public IReadOnlyCollection<IAchievementEffect> Effects { get; }
    public DateTime UtcNow { get; }

    public Swine Swine { get; }

    public FeedGenerator(ILogger<FeedGenerator> logger, UserContext context, IAchievementController achievController, IThrowupCalculatorFactory throwupCalcFactory, int swineId)
    {
        Logger = logger;
        Context = context;
        AchievController = achievController;

        Swine = Context.Swines.First(s => s.SwineId == swineId);

        var swineInfoId = Context.Infos.First(i => i.SwineId == Swine.SwineId).InfoId;

        Effects = Context.Achievements
            .Where(a => a.SwineInfoId == swineInfoId)
            .AsEnumerable()
            .Select(a => AchievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToList();

        UtcNow = DateTime.Now.ToUniversalTime();
        ThrowupCalculator = throwupCalcFactory.Create(UtcNow, Effects);
    }

    public FeedResult Generate()
    {
        var recentFeeds = Context.Feeds
            .Where(f => f.SwineId == Swine.SwineId)
            .AsEnumerable()
            .Where(f => (UtcNow - f.DateTime).TotalHours < OVERFEED_COOLDOWN)
            .ToList();

        Result result = RollResult(recentFeeds);
        if (result == Result.Full)
            return FeedResult.Full;

        double luck = RollLuck();
        int absAmount = RollAmount(luck);
        int amount = ApplyResult(recentFeeds, absAmount, result);
        Logger.LogInformation("Final amount: {amount}", amount);

        var oldWeight = Swine.Weight;
        return new FeedResult()
        {
            Luck = luck,
            Amount = amount,
            Result = result,
            OldWeight = oldWeight,
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

        var recentThrowups = Context.WeightLosses
            .Where(wl => wl.SwineId == Swine.SwineId)
            .Where(wl => wl.IsThrowUp)
            .AsEnumerable()
            .Where(wl => (UtcNow - wl.DateTime).TotalHours < THROWUP_COOLDOWN);

        if (recentThrowups.Any())
            return Result.Full;

        if (recentFeeds.Any() == false)
            return Result.FirstFeed;

        return ThrowupCalculator.IsThrowup(recentFeeds) ? Result.Throwup : Result.Overfeed;
    }

    private int ApplyAmountEffects(int amount)
    {
        var totalSlaughteredWeight = Context.Slaughters
            .Where(s => s.UserId == Swine.OwnerId)
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

