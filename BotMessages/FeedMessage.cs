using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class FeedMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines.Include(s => s.Feeds)
            .First(s => s.OwnerId == userId);

        int oldWeight = swine.Weight;
        var luck = Random.Shared.Next(1, 21);
        var amountMod = Random.Shared.Next(-2, 3);
        var amount = Math.Max(1, luck + amountMod);

        int newWeight = oldWeight + amount;

        swine.Weight = newWeight;
        swine.Feeds.Add(new Feed()
        {
            DateTime = DateTime.Now,
            Amount = amount,
        });

        if (amount < 5)
        {
            Text.Italic("К сожалению, ")
                .Bold(swine.Name)
                .Italic(" сегодня ночью приснился кошмар, поэтому он весь день в плохом настроении и почти не ест...").LineBreak();
        }
        else if (amount > 15)
        {
            Text.Italic("Сегодня ")
                .Bold(swine.Name)
                .Italic(" проснулся с отличным аппетитом и радостно хрюкает при вашем приближении!").LineBreak();
        }
        else
        {
            Text.Bold(swine.Name)
                .Italic(" спокойно ест из своей кормушки.").LineBreak();
        }

        Text
            .LineBreak()
            .Bold($"{oldWeight} кг + {amount} кг → {newWeight} кг");

        return Task.CompletedTask;
    }
}
