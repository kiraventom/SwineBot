using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public interface IFeedGeneratorFactory
{
    IFeedGenerator Create(UserContext context, int swineId);
}

public class FeedGeneratorFactory(IServiceProvider sp) : IFeedGeneratorFactory
{
    public IFeedGenerator Create(UserContext context, int swineId) => ActivatorUtilities.CreateInstance<FeedGenerator>(sp, context, swineId);
}

public interface IFeedGenerator
{
    FeedResult Generate();
}

public class FeedGenerator : IFeedGenerator
{
    public const int OVERFEED_COOLDOWN = 24;
    public const int THROWUP_COOLDOWN = 24;

    private ILogger<FeedGenerator> Logger { get; }
    private UserContext Context { get; }
    private IAchievementController AchievController { get; }
    private IThrowupCalculator ThrowupCalculator { get; }
    private int SwineId { get; }

    public IReadOnlyCollection<IAchievementEffect> Effects { get; }
    public DateTime UtcNow { get; }

    public FeedGenerator(ILogger<FeedGenerator> logger, UserContext context, IAchievementController achievController, IDateTimeNowProvider dtnProvider, IThrowupCalculatorFactory throwupCalcFactory, int swineId)
    {
        Logger = logger;
        Context = context;
        AchievController = achievController;
        SwineId = swineId;

        var swine = Context.Swines.First(s => s.SwineId == swineId);
        var swineInfoId = Context.Infos.First(i => i.SwineId == swineId).InfoId;

        Effects = Context.Achievements
            .AsNoTracking()
            .Where(a => a.SwineInfoId == swineInfoId)
            .AsEnumerable()
            .Select(a => AchievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToList();

        UtcNow = dtnProvider.UtcNow;
        ThrowupCalculator = throwupCalcFactory.Create(UtcNow, Effects);
    }

    public FeedResult Generate()
    {
        var swine = Context.Swines.First(s => s.SwineId == SwineId);
        var recentFeeds = Context.GetRecentFeeds(SwineId, UtcNow);

        Result result = RollResult(recentFeeds);
        if (result == Result.Full)
            return FeedResult.Full;

        double luck = RollLuck();
        int absAmount = RollAmount(luck);
        int amount = ApplyResult(recentFeeds, absAmount, result);
        Logger.LogInformation("Final amount: {amount}", amount);

        var oldWeight = swine.Weight;

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

        return luck;
    }

    private double ApplyLuckEffects(double luck)
    {
        foreach (var effect in Effects.OfType<NoOverfeedsLuckAmplifierEffect>())
        {
            var oldLuck = luck;
            luck = effect.Apply(Context, SwineId, luck);
            if (luck != oldLuck)
                Logger.LogInformation("Applied effect {effect}, luck changed from {old} to {new}", effect.Type.ToString(), oldLuck, luck);
        }

        return luck;
    }

    private int RollAmount(double luck)
    {
        const int MAX_AMOUNT = 20;
        var baseAmount = (int)Math.Round(MAX_AMOUNT * luck);
        baseAmount = Math.Max(1, baseAmount);
        Logger.LogInformation("Base amount rolled: {baseAmount}", baseAmount);
        var amount = ApplyGrowthModifier(baseAmount);
        return amount;
    }

    private Result RollResult(IReadOnlyCollection<Feed> recentFeeds)
    {
        var recentThrowups = Context.GetRecentThrowups(SwineId, UtcNow);

        if (recentThrowups.Count != 0)
            return Result.Full;

        var lastThrowup = Context.WeightLosses
            .AsNoTracking()
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefault();

        if (recentFeeds.Count == 0 && lastThrowup is not { Ignored: true })
            return Result.FirstFeed;

        return ThrowupCalculator.IsThrowup(recentFeeds) ? Result.Throwup : Result.Overfeed;
    }

    private int ApplyGrowthModifier(int amount)
    {
        var swine = Context.Swines.First(s => s.SwineId == SwineId);

        var totalSlaughteredWeight = Context.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .Where(s => s.GroupId == swine.GroupId)
            .Where(s => s.SwineWeight >= SlaughterMessage.MIN_SWINE_WEIGHT)
            .Sum(s => s.SwineWeight);

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        var amountEff = amount * growthMod;
        var rounded = (int)Math.Round(amountEff);
        Logger.LogInformation("Amount with growth mod: {amount} * {mod} = {amountEff}; rounded to {rounded}", amount, growthMod, amountEff, rounded);
        return rounded;
    }

    private int ApplyResult(IReadOnlyCollection<Feed> recentFeeds, int amount, Result result)
    {
        var swine = Context.Swines.First(s => s.SwineId == SwineId);
        if (result == Result.Throwup)
            return ThrowupCalculator.Calculate(recentFeeds, swine.Weight, amount) * -1;

        return amount;
    }
}

