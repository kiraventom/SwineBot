using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class TopMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var groupId = userContext.Swines.First(s => s.SwineId == swineId).GroupId;

        var topSwines = userContext.Swines
            .Where(s => s.GroupId == groupId)
            .OrderByDescending(s => s.Weight)
            .Take(10)
            .Where(s => s.Weight > 1);

        Text.Bold("Топ 10 свинов")
            .LineBreak().LineBreak();

        int counter = 1;
        bool isSenderSwineInTop = false;

        foreach (var swine in topSwines)
        {
            if (swine.SwineId == swineId)
                isSenderSwineInTop = true;

            OutputSwine(counter++, swine, swine.SwineId == swineId);
        }

        if (isSenderSwineInTop == false)
        {
            var senderSwine = userContext.Swines.First(s => s.SwineId == swineId);
            var senderIndex = userContext.Swines
                .OrderByDescending(s => s.Weight)
                .ToList()
                .IndexOf(senderSwine);

            Text.Verbatim("...").LineBreak();
            OutputSwine(senderIndex + 1, senderSwine, true);
        }

        return Task.CompletedTask;
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




