using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Achievements.Effects;
using SwineBot.Actions.Commands;
using SwineBot.Model;

namespace SwineBot.BotMessages.Feed;

public interface IFeedGeneratorFactory
{
    Task<IFeedGenerator> Create(int? swineId);
}

public class FeedGeneratorFactory(ILogger<FeedGenerator> logger, UserContext context, AchievementController achievController, IDateTimeNowProvider dtnProvider, IThrowupCalculatorFactory throwupCalcFactory) : IFeedGeneratorFactory
{
    public async Task<IFeedGenerator> Create(int? swineId)
    { 
        var gen = new FeedGenerator(logger, context, achievController, dtnProvider, throwupCalcFactory);
        await gen.Init(swineId.Value);
        return gen;
    }
}

public interface IFeedGenerator
{
    Task<FeedResult> Generate();
}

public class FeedGenerator : IFeedGenerator
{
    public const int OVERFEED_COOLDOWN = 24;
    public const int THROWUP_COOLDOWN = 24;

    private ILogger<FeedGenerator> Logger { get; }
    private UserContext Context { get; }
    private AchievementController AchievController { get; }
    private IThrowupCalculatorFactory ThrowupCalcFactory { get; }

    private int SwineId { get; set; }
    private IThrowupCalculator ThrowupCalculator { get; set; }
    private IReadOnlyCollection<IAchievementEffect> Effects { get; set; }
    private DateTime UtcNow { get; }

    public FeedGenerator(ILogger<FeedGenerator> logger, UserContext context, AchievementController achievController, IDateTimeNowProvider dtnProvider, IThrowupCalculatorFactory throwupCalcFactory)
    {
        Logger = logger;
        Context = context;
        AchievController = achievController;
        ThrowupCalcFactory = throwupCalcFactory;
        UtcNow = dtnProvider.UtcNow;
    }

    public async Task Init(int swineId)
    {
        SwineId = swineId;

        var swineInfoId = (await Context.Infos.FirstAsync(i => i.SwineId == SwineId)).InfoId;

        Effects = await Context.Achievements
            .Where(a => a.SwineInfoId == swineInfoId)
            .AsAsyncEnumerable()
            .Select(a => AchievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToListAsync();

        ThrowupCalculator = ThrowupCalcFactory.Create(UtcNow, Effects);
    }

    public async Task<FeedResult> Generate()
    {
        var swine = await Context.Swines.FirstAsync(s => s.SwineId == SwineId);
        var recentFeeds = await Context.GetRecentFeeds(SwineId, UtcNow);

        Result result = await RollResult(recentFeeds);
        if (result == Result.Full)
            return FeedResult.Full;

        double luck = await RollLuck();
        int absAmount = await RollAmount(luck);
        int amount = await ApplyResult(recentFeeds, absAmount, result);
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

    private async Task<double> RollLuck()
    {
        var baseLuck = Random.Shared.NextDouble();
        Logger.LogInformation("Luck rolled: {luck}", baseLuck);

        var luck = await ApplyLuckEffects(baseLuck);
        return luck;
    }

    private async Task<double> ApplyLuckEffects(double luck)
    {
        foreach (var effect in Effects.OfType<NoOverfeedsLuckAmplifierEffect>())
        {
            var oldLuck = luck;
            luck = await effect.Apply(Context, SwineId, luck);
            if (luck != oldLuck)
                Logger.LogInformation("Applied effect {effect}, luck changed from {old} to {new}", effect.Type.ToString(), oldLuck, luck);
        }

        return luck;
    }

    private async Task<int> RollAmount(double luck)
    {
        const int MAX_AMOUNT = 20;
        var baseAmount = MAX_AMOUNT * luck;
        var nonZeroBaseAmount = Math.Max(1, baseAmount);
        Logger.LogInformation("Base amount rolled: {luck} * {maxAmount} = {baseAmount}", luck, MAX_AMOUNT, baseAmount);

        if (baseAmount != nonZeroBaseAmount)
            Logger.LogInformation("Base amount adjusted to not being zero from {baseAmount} to {nonZero}", baseAmount, nonZeroBaseAmount);

        var amount = await ApplyGrowthModifier(nonZeroBaseAmount);
        return amount;
    }

    private async Task<Result> RollResult(IReadOnlyCollection<Model.Feed> recentFeeds)
    {
        var recentThrowups = await Context.GetRecentThrowups(SwineId, UtcNow);

        if (recentThrowups.Count != 0)
            return Result.Full;

        var lastThrowup = await Context.WeightLosses
            .Where(wl => wl.SwineId == SwineId)
            .Where(wl => wl.IsThrowUp)
            .OrderByDescending(wl => wl.DateTime)
            .FirstOrDefaultAsync();

        if (recentFeeds.Count != 0 || lastThrowup is { Amount: 0 } /*Ignored*/)
            return await ThrowupCalculator.IsThrowup(recentFeeds) ? Result.Throwup : Result.Overfeed;

        return Result.FirstFeed;
    }

    private async Task<int> ApplyGrowthModifier(double amount)
    {
        var swine = await Context.Swines.FirstAsync(s => s.SwineId == SwineId);

        var slaughters = Context.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .Where(s => s.GroupId == swine.GroupId)
            .Where(s => s.SwineWeight >= SlaughterCommand.MIN_SWINE_WEIGHT);

        var totalSlaughteredWeight = await slaughters.CountAsync() > 0 ? await slaughters.SumAsync(s => s.SwineWeight) : 0;

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        var amountEff = amount * growthMod;
        var rounded = (int)Math.Round(amountEff);
        Logger.LogInformation("Amount with growth mod: {amount} * {mod} = {amountEff}; rounded to {rounded}", amount, growthMod, amountEff, rounded);
        return rounded;
    }

    private async Task<int> ApplyResult(IReadOnlyCollection<Model.Feed> recentFeeds, int amount, Result result)
    {
        var swine = await Context.Swines.FirstAsync(s => s.SwineId == SwineId);
        if (result == Result.Throwup)
            return await ThrowupCalculator.Calculate(recentFeeds, swine.Weight, amount) * -1;

        return amount;
    }
}

