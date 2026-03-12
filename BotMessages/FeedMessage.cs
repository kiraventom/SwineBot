using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.Achievements.Effects;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class FeedMessage(ILogger logger, AchievementController achievController) : BotMessage(logger)
{
    public const int THROWUP_COOLDOWN = 24;
    public const int OVERFEED_COOLDOWN = 24;

    private const int MIN_LUCK = 1;
    private const int MAX_LUCK = 21;

    private const int MIN_AMOUNT_MOD = -2;
    private const int MAX_AMOUNT_MOD = 3;

    private const double OVERFEED_THROWUP_BASE_CHANCE = 0.01;
    private const double BASE_OVERFEED_SCALE = 2.5;
    private const int LOW_AMOUNT = 5;
    private const int HIGH_AMOUNT = 15;

    private const int OVERFEED_FADEOUT_HOURS = 12;
    private const double OVERFEED_FADEOUT_SCALE = 0.75;
    private const double FADED_OUT_OVERFEED_SCALE = BASE_OVERFEED_SCALE * OVERFEED_FADEOUT_SCALE;

    public int OldWeight { get; private set; }
    public int Amount { get; private set; }
    public int NewWeight => OldWeight + Amount;

    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var owner = userContext.Users.First(u => u.UserId == userId);

        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
            .First(s => s.OwnerId == userId);

        var effects = swine.Stats.Achievements
            .Select(a => achievController.GetLevel(a))
            .Where(a => a.Effect != null)
            .Select(a => a.Effect)
            .ToList();

        var now = DateTime.Now.ToUniversalTime();
        var recentThrowups = swine.WeightLosses
            .Where(wl => (now - wl.DateTime).TotalHours < THROWUP_COOLDOWN)
            .Where(wl => wl.IsThrowUp);

        if (recentThrowups.Any())
        {
            Text.Verbatim("После недавнего инцидента с перееданием ")
                .Bold(swine.Name)
                .Verbatim(" совсем не до еды...");

            return Task.CompletedTask;
        }

        OldWeight = swine.Weight;
        var recentFeeds = swine.Feeds.Where(f => (now - f.DateTime).TotalHours < OVERFEED_COOLDOWN).ToList();
        bool isFirstFeed = recentFeeds.Any() == false;

        var luck = Random.Shared.Next(MIN_LUCK, MAX_LUCK);
        var amountMod = Random.Shared.Next(MIN_AMOUNT_MOD, MAX_AMOUNT_MOD);

        var totalSlaughteredWeight = userContext.Slaughters
            .Where(s => s.UserId == owner.UserId)
            .Sum(s => s.SwineWeight);

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        var baseAmount = Math.Max(1, luck + amountMod);
        Amount = (int)(Math.Ceiling(baseAmount * growthMod));

        if (!isFirstFeed)
        {
            var overfeedScale = GetOverfeedScale(recentFeeds, now, effects.OfType<OverfeedScaleModifierEffect>());
            var throwupThreshold = OVERFEED_THROWUP_BASE_CHANCE * Math.Pow(overfeedScale, recentFeeds.Count);
            throwupThreshold = Math.Min(0.99, throwupThreshold);

            var overfeedChance = Random.Shared.NextDouble();
            if (overfeedChance < throwupThreshold)
            {
                Logger.Information("Overfeed: {overfeed} < {throwup}", overfeedChance, throwupThreshold);
                var amountLost = GetThrowup(recentFeeds, effects);
                Amount = amountLost * -1;

                swine.Weight = NewWeight;
                swine.WeightLosses.Add(new WeightLoss()
                {
                    DateTime = now,
                    IsThrowUp = true,
                    Amount = Amount
                });

                if (Amount == 0)
                {
                    Text
                        .Bold(swine.Name)
                        .Verbatim(" уже почти стошнило, но в последний момент свин сдержал позыв и, нахмурившись, утопал обратно на своё место.")
                        .LineBreak()
                        .LineBreak()
                        .Bold($"Вес не изменился: {NewWeight} кг");

                    var consecutiveOverfeeds = OverfeedAchievementChecker.CountConsecutiveOverfeeds(userContext, swine.SwineId);
                    if (consecutiveOverfeeds != 0)
                    {
                        Text
                            .LineBreak()
                            .Italic($"Это происшествие не нарушит ваш стрик в {consecutiveOverfeeds} {MessageTextUtils.GetDeclinatedNoun(consecutiveOverfeeds, Unit.Overfeed)}");
                    }
                }
                else
                {
                    Text
                        .Verbatim("Едва глаза ")
                        .Bold(swine.Name)
                        .Verbatim(" увидели еду, всё его тело содрогнулось в рвотном позыве... Заблевав всю кормушку, изрядно исхудавший хряк грустно вернулся в глубину хлева.")
                        .LineBreak()
                        .LineBreak()
                        .Bold($"{OldWeight} кг - {amountLost} кг → {NewWeight} кг");
                }

                return Task.CompletedTask;
            }

            Logger.Information("No overfeed: {overfeed} >= {throwup}", overfeedChance, throwupThreshold);
        }

        swine.Weight = NewWeight;
        swine.Feeds.Add(new Feed()
        {
            DateTime = now,
            Amount = Amount,
        });

        if (baseAmount < LOW_AMOUNT)
        {
            if (isFirstFeed)
            {
                Text.Verbatim("К сожалению, ")
                    .Bold(swine.Name)
                    .Verbatim(" сегодня ночью приснился кошмар, поэтому он выглядит угрюмым и почти не ест...").LineBreak();
            }
            else
            {
                Text.Bold(swine.Name).Verbatim(", явно сытый, неохотно жуёт очередную порцию...").LineBreak();
            }
        }
        else if (baseAmount > HIGH_AMOUNT)
        {
            if (isFirstFeed)
            {
                Text.Verbatim("Сегодня ")
                    .Bold(swine.Name)
                    .Verbatim(" проснулся с отличным аппетитом и радостно хрюкает при вашем приближении!").LineBreak();
            }
            else
            {
                Text.Verbatim("Как ни в чём ни бывало, ")
                    .Bold(swine.Name)
                    .Verbatim(" налетает на новую порцию!").LineBreak();
            }
        }
        else
        {
            if (isFirstFeed)
            {
                Text.Bold(swine.Name)
                    .Verbatim(" спокойно ест из своей кормушки.").LineBreak();
            }
            else
            {
                Text.Bold(swine.Name)
                    .Verbatim(" довольно поедает добавку.").LineBreak();
            }
        }

        Text
            .LineBreak()
            .Bold($"{OldWeight} кг + {Amount} кг → {NewWeight} кг");

        if (!isFirstFeed)
        {
            var recentFeedsCount = recentFeeds.Count + 1;
            var feedDecl = MessageTextUtils.GetDeclinatedNoun(recentFeedsCount, Unit.Meal);
            Text.LineBreak()
                .Italic($"⚠ Перекорм! {recentFeedsCount} {feedDecl} пищи за последние {OVERFEED_COOLDOWN} часа!");
        }

        return Task.CompletedTask;
    }

    private int GetThrowup(IEnumerable<Feed> recentFeeds, IReadOnlyCollection<IAchievementEffect> effects)
    {
        int amountLost = Math.Min(OldWeight - 1, recentFeeds.Sum(f => f.Amount) + Amount);
        int initialAmountLost = amountLost;

        foreach (var effect in effects.OfType<ThrowupScaleEffect>())
            amountLost = effect.Apply(amountLost);

        foreach (var effect in effects.OfType<ThrowupIgnoreChanceEffect>())
            amountLost = effect.Apply(amountLost);

        if (amountLost != initialAmountLost)
            Log.Logger.Information("Throwup changed from {base} to {new}", initialAmountLost, amountLost);

        return amountLost;
    }

    private static double GetOverfeedScale(IEnumerable<Feed> recentFeeds, DateTime utcNow, IEnumerable<OverfeedScaleModifierEffect> effects)
    {
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

        foreach (var effect in effects)
            overfeedScale = effect.Apply(overfeedScale);

        if (overfeedScale != initialOverfeedScale)
            Log.Logger.Information("Effects: Overfeed scale changed from {base} to {new}", initialOverfeedScale, overfeedScale);

        return overfeedScale;
    }
}
