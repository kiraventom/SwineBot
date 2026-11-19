using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands.Duel;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/feed", "Покормить своего свина \U0001F416")]
public class FeedCommand(ILogger<FeedCommand> logger, IMessageFactory messageFactory, IFeedGeneratorFactory feedGeneratorFactory, UserContext context, AchievementController achievController) : Command<FeedMessage, FeedViewModel>(messageFactory, achievController), IActionCommand
{
    public const double LOW_LUCK_THRESHOLD = 0.15;
    public const double HIGH_LUCK_THRESHOLD = 0.85;

    protected override async Task<FeedViewModel> ExecuteInternal(Update update, string parameter)
    {
        var feedManager = await feedGeneratorFactory.Create(update.SwineId);
        var result = await feedManager.Generate();

        var feedViewModel = result.Result switch
        {
            Result.FirstFeed => await HandleFeed(result, update),
            Result.Overfeed => await HandleFeed(result, update),
            Result.Throwup => await HandleThrowup(result, update),
            Result.Full => await HandleFull(result, update),
            _ => throw new NotSupportedException($"Unexpected feed result type \"{result.Result.ToString()}\"")
        };

        await context.SaveChangesAsync();

        // DBG
        if (feedViewModel.Result.Result != Result.FirstFeed && feedViewModel.Result.RecentFeedsCount == 1)
        {
            logger.LogWarning("Not first feed but 1 consecutive feed, something is wrong");
        }

        return feedViewModel;
    }

    private async Task<FeedViewModel> HandleFull(FeedResult result, Update update)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        return new FeedViewModel(result, swine.Name, 0);
    }

    private async Task<FeedViewModel> HandleThrowup(FeedResult result, Update update)
    {
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);
        swine.Weight = result.NewWeight;

        context.WeightLosses.Add(new WeightLoss()
        {
            SwineId = swine.SwineId,
            DateTime = result.UtcDT,
            IsThrowUp = true,
            Amount = result.Amount,
            Luck = result.Luck
        });

        var consecutiveOverfeeds = await OverfeedChecker.CountConsecutiveOverfeeds(context, swine.SwineId);

        return new FeedViewModel(result, swine.Name, consecutiveOverfeeds);
    }

    private async Task<FeedViewModel> HandleFeed(FeedResult result, Update update)
    {
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);

        // Дебафф за отказ (страх-голод) убран в v3: отказ платится славой, а не едой.
        // Блок оставлен выключенным, чтобы старые конфиги вели себя предсказуемо.
        if (DuelBalance.Debuff == DuelDebuff.FeedSteal && swine.Glory < 0)
        {
            // Шанс поесть падает как 0.75^n (без нижней границы): чем глубже долг, тем чаще трусит
            var debt = -swine.Glory;
            var eatChance = Math.Pow(DuelBalance.FearKeepChancePerDebt, debt);
            var fearChance = 1 - eatChance;

            if (Random.Shared.NextDouble() < fearChance)
            {
                var activeSince = result.UtcDT.AddHours(-DuelBalance.ActiveFeederHours);
                var activeIds = await context.Feeds
                    .Where(f => f.DateTime > activeSince && f.SwineId != swine.SwineId)
                    .Select(f => f.SwineId)
                    .Distinct()
                    .ToListAsync();

                if (activeIds.Count > 0)
                {
                    var recipients = activeIds
                        .OrderBy(_ => Random.Shared.Next())
                        .Take(DuelBalance.FearRecipients)
                        .ToList();

                    var share = result.Amount / recipients.Count;
                    var remainder = result.Amount - share * recipients.Count;

                    var receivers = await context.Swines.AsTracking()
                        .Where(s => recipients.Contains(s.SwineId))
                        .ToListAsync();

                    foreach (var receiver in receivers)
                        receiver.Weight += share;

                    if (receivers.Count > 0)
                        receivers[0].Weight += remainder;

                    // Неудачная попытка поесть сжигает одно очко отрицательной отваги:
                    // свин потихоньку успокаивается
                    if (DuelBalance.FearBurnsDebt && swine.Glory < 0)
                        swine.Glory += 1;

                    logger.LogInformation(
                        "Swine [{swine}] got scared (glory now {glory}): {amount} kg split among [{receivers}]",
                        swine.SwineId, swine.Glory, result.Amount, string.Join(", ", recipients));

                    return new FeedViewModel(result, swine.Name, 0);
                }
            }
        }

        swine.Weight = result.NewWeight;

        // Множитель разгоняет свина, пока он лёгкий, и тратится за кормёжки, когда
        // он уже забрался выше порога веса — иначе он становится вечным двигателем
        var modifierUsed = await context.ApplyFeedMultiplierDecayAsync(swine, result.BaseAmount, result.Amount);
        logger.LogInformation("Feed multiplier {modifier} for swine [{swine}] at {weight} kg, fuel now {fuel}",
            modifierUsed, swine.SwineId, swine.Weight, swine.MultiplierFuel);

        context.Feeds.Add(new Model.Feed()
        {
            SwineId = swine.SwineId,
            DateTime = result.UtcDT,
            Amount = result.Amount,
            Luck = result.Luck
        });

        return new FeedViewModel(result, swine.Name, 0);
    }
}

