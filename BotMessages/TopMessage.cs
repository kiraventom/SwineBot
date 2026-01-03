using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class TopMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var topSwines = userContext.Swines
            .OrderByDescending(s => s.Weight)
            .Take(10);

        Text.Bold("Топ 10 свинов")
            .LineBreak().LineBreak();

        int counter = 0;
        bool isSenderSwineInTop = false;

        var senderSwine = userContext.Swines.First(s => s.OwnerId == userId);
        foreach (var swine in topSwines)
        {
            if (swine == senderSwine)
                isSenderSwineInTop = true;

            OutputSwine(counter++, swine);
        }

        if (isSenderSwineInTop == false)
        {
            var senderIndex = userContext.Swines
                .OrderByDescending(s => s.Weight)
                .ToList()
                .IndexOf(senderSwine);

            Text.Verbatim("...").LineBreak();
            OutputSwine(senderIndex + 1, senderSwine);
        }

        return Task.CompletedTask;
    }

    private void OutputSwine(int rank, Swine swine)
    {
        Text.Verbatim(rank)
            .Verbatim(". ")
            .Bold(swine.Name)
            .Verbatim(": ")
            .Verbatim($"{swine.Weight} кг")
            .LineBreak();
    }
}




