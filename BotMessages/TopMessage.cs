using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class TopMessage : BotMessage<TopViewModel>
{
    public override void Init<T>(ILogger<T> logger, TopViewModel viewModel)
    {
        Text.Bold("Топ 10 свинов")
            .LineBreak().LineBreak();

        int counter = 1;
        bool isSenderSwineInTop = false;

        foreach (var swine in viewModel.TopSwines)
        {
            if (swine.SwineId == viewModel.SenderSwine.SwineId)
                isSenderSwineInTop = true;

            OutputSwine(counter++, swine, swine.SwineId == viewModel.SenderSwine.SwineId);
        }

        if (isSenderSwineInTop == false)
        {
            Text.Verbatim("...").LineBreak();
            OutputSwine(viewModel.SenderIndex + 1, viewModel.SenderSwine, true);
        }
    }

    private void OutputSwine(int rank, Swine swine, bool isSender)
    {
        var swineName = SwineUtils.GetShortName(swine);

        if (isSender)
        {
            Text.Bold(rank)
                .Bold(". ")
                .Bold(swineName)
                .Bold(": ")
                .Bold($"{swine.Weight} кг")
                .LineBreak();
        }
        else
        {
            Text.Verbatim(rank)
                .Verbatim(". ")
                .Bold(swineName)
                .Verbatim(": ")
                .Verbatim($"{swine.Weight} кг")
                .LineBreak();
        }
    }
}
