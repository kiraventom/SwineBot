using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class TopMessage(ILogger<TopMessage> logger, UserContext context) : BotMessage(logger)
{
    protected override async Task InitInternal(Update update)
    {
        var topSwines = context.Swines
            .Where(s => s.GroupId == update.GroupId)
            .OrderByDescending(s => s.Weight)
            .Take(10)
            .Where(s => s.Weight > 1);

        Text.Bold("Топ 10 свинов")
            .LineBreak().LineBreak();

        int counter = 1;
        bool isSenderSwineInTop = false;

        foreach (var swine in topSwines)
        {
            if (swine.SwineId == update.SwineId)
                isSenderSwineInTop = true;

            OutputSwine(counter++, swine, swine.SwineId == update.SwineId);
        }

        if (isSenderSwineInTop == false)
        {
            var senderSwine = context.Swines.First(s => s.SwineId == update.SwineId);
            var senderIndex = (await context.Swines
                .OrderByDescending(s => s.Weight)
                .ToListAsync())
                .IndexOf(senderSwine);

            Text.Verbatim("...").LineBreak();
            OutputSwine(senderIndex + 1, senderSwine, true);
        }
    }

    private void OutputSwine(int rank, Swine swine, bool isSender)
    {
        if (isSender)
        {
            Text.Bold(rank)
                .Bold(". ")
                .Bold(swine.Name)
                .Bold(": ")
                .Bold($"{swine.Weight} кг")
                .LineBreak();
        }
        else
        {
            Text.Verbatim(rank)
                .Verbatim(". ")
                .Bold(swine.Name)
                .Verbatim(": ")
                .Verbatim($"{swine.Weight} кг")
                .LineBreak();
        }
    }
}




