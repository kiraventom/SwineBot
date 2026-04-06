using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Start;

public class StartMessage : BotMessage<StartViewModel>
{
    public override void Init<T>(ILogger<T> logger, StartViewModel viewModel)
    {
        Text.Bold("\U0001F437 Бот с кормлением свинок \U0001F43D").LineBreak()
            .LineBreak()
            .Italic("Доступные команды:").LineBreak();

        foreach (var info in viewModel.CommandInfos)
            Text.Verbatim(info.Title).Verbatim(" - ").Verbatim(info.Description).LineBreak();
    }
}
