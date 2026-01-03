using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class FeedMessage(ILogger logger) : BotMessage(logger)
{
    private const int THROWUP_COOLDOWN = 24;
    private const int OVERFEED_COOLDOWN = 24;

    private const int MIN_LUCK = 1;
    private const int MAX_LUCK = 21;

    private const int MIN_AMOUNT_MOD = -2;
    private const int MAX_AMOUNT_MOD = 3;

    private const double OVERFEED_THROWUP_BASE_CHANCE = 0.01;
    private const double OVERFEED_SCALE = 2.5;
    private const int LOW_AMOUNT = 5;
    private const int HIGH_AMOUNT = 15;

    public int OldWeight { get; private set; }
    public int Amount { get; private set; }
    public int NewWeight => OldWeight + Amount;

    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.OwnerId == userId);

        var now = DateTime.Now;
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
        Amount = Math.Max(1, luck + amountMod);

        if (!isFirstFeed)
        {
            var throwupThreshold = OVERFEED_THROWUP_BASE_CHANCE * Math.Pow(OVERFEED_SCALE, recentFeeds.Count);
            throwupThreshold = Math.Min(0.99, throwupThreshold);

            var overfeedChance = Random.Shared.NextDouble();
            Logger.Information("Overfeed: {overfeed} / {throwup}", overfeedChance, throwupThreshold);
            if (overfeedChance < throwupThreshold)
            {
                var amountLost = Math.Min(OldWeight - 1, recentFeeds.Sum(f => f.Amount) + Amount);
                Amount = amountLost * -1;

                swine.Weight = NewWeight;
                swine.WeightLosses.Add(new WeightLoss()
                {
                    DateTime = now,
                    IsThrowUp = true,
                    Amount = Amount
                });

                Text
                    .Verbatim("Едва глаза ")
                    .Bold(swine.Name)
                    .Verbatim(" увидели еду, всё его тело содрогнулось в рвотном позыве... Заблевав всю кормушку, изрядно исхудавший хряк грустно вернулся в глубину хлева.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"{OldWeight} кг - {amountLost} кг → {NewWeight} кг");

                return Task.CompletedTask;
            }
        }

        swine.Weight = NewWeight;
        swine.Feeds.Add(new Feed()
        {
            DateTime = now,
            Amount = Amount,
        });

        if (Amount < LOW_AMOUNT)
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
        else if (Amount > HIGH_AMOUNT)
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
            var feedDecl = MessageTextUtils.GetDeclinatedNoun(recentFeedsCount, "приём", "приёма", "приёмов");
            Text.LineBreak()
                .Italic($"⚠ Перекорм! {recentFeedsCount} {feedDecl} пищи за последние {OVERFEED_COOLDOWN} часа!");
        }

        return Task.CompletedTask;
    }
}
