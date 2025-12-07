using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class FeedMessage(ILogger logger) : BotMessage(logger)
{
    private const double OVERFEED_THROWUP_BASE_CHANCE = 0.01;

    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.OwnerId == userId);

        var now = DateTime.Now;
        var recentThrowups = swine.WeightLosses
            .Where(wl => (now - wl.DateTime).TotalHours < 24)
            .Where(wl => wl.IsThrowUp);

        if (recentThrowups.Any())
        {
            Text.Verbatim("После недавнего инцидента с перееданием ")
                .Bold(swine.Name)
                .Verbatim(" совсем не до еды...");

            return Task.CompletedTask;
        }

        int oldWeight = swine.Weight;
        int newWeight;
        var recentFeeds = swine.Feeds.Where(f => (now - f.DateTime).TotalHours < 24).ToList();
        bool isFirstFeed;
        if (recentFeeds.Any())
        {
            var lastFeedDT = recentFeeds.Max(f => f.DateTime);
            var diff = (now - lastFeedDT).TotalHours;
            isFirstFeed = diff > 24;
        }
        else
        {
            isFirstFeed = true;
        }

        var luck = Random.Shared.Next(1, 21);
        var amountMod = Random.Shared.Next(-2, 3);
        var amount = Math.Max(1, luck + amountMod);

        if (!isFirstFeed)
        {
            var throwup = OVERFEED_THROWUP_BASE_CHANCE * Math.Pow(4, recentFeeds.Count - 1);

            var overfeed = Random.Shared.NextDouble();
            Logger.Information("Overfeed: {overfeed} : {throwup}", overfeed, throwup);
            if (overfeed < throwup)
            {
                var amountLost = Math.Min(oldWeight - 1, recentFeeds.Sum(f => f.Amount) + amount);
                newWeight = oldWeight - amountLost;

                swine.Weight = newWeight;
                swine.WeightLosses.Add(new WeightLoss()
                {
                    DateTime = now,
                    IsThrowUp = true,
                    Amount = amountLost
                });

                Text
                    .Verbatim("Едва глаза ")
                    .Bold(swine.Name)
                    .Verbatim(" увидели еду, всё его тело содрогнулось в рвотном позыве... Заблевав всю кормушку, изрядно исхудавший хряк грустно вернулся в глубину хлева.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"{oldWeight} кг - {amountLost} кг → {newWeight} кг");

                return Task.CompletedTask;
            }
        }

        newWeight = oldWeight + amount;

        swine.Weight = newWeight;
        swine.Feeds.Add(new Feed()
        {
            DateTime = now,
            Amount = amount,
        });

        if (amount < 5)
        {
            if (isFirstFeed)
            {
                Text.Verbatim("К сожалению, ")
                    .Bold(swine.Name)
                    .Verbatim(" сегодня ночью приснился кошмар, поэтому он весь день в плохом настроении и почти не ест...").LineBreak();
            }
            else
            {
                Text.Bold(swine.Name).Verbatim(", явно сытый, неохотно жуёт очередную порцию...").LineBreak();
            }
        }
        else if (amount > 15)
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
            .Bold($"{oldWeight} кг + {amount} кг → {newWeight} кг");

        if (!isFirstFeed)
        {
            var recentFeedsCount = recentFeeds.Count + 1;
            var feedDecl = MessageTextUtils.GetDeclinatedNoun(recentFeedsCount, "приём", "приёма", "приёмов");
            Text.LineBreak()
                .Italic($"⚠ Перекорм! {recentFeedsCount} {feedDecl} пищи за последние 24 часа!");
        }

        return Task.CompletedTask;
    }
}
