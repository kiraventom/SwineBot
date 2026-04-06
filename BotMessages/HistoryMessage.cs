using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class HistoryMessage : BotMessage<HistoryViewModel>
{
    public override void Init<T>(ILogger<T> logger, HistoryViewModel viewModel)
    {
        Text.Italic("История веса свинок");
        PhotoBytes = viewModel.PlotBytes;
    }
}
