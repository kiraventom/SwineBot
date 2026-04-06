using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class NewNameMessage : BotMessage<SetNameViewModel>
{
    public override void Init<T>(ILogger<T> logger, SetNameViewModel viewModel)
    {
        if (!viewModel.IsNameProvided)
        {
            Text.Italic("Формат команды:")
                .LineBreak()
                .Monospace($"{SetNameCommand.COMMAND_NAME} <новое имя>");

            return;
        }

        if (viewModel.IsNameTheSame)
        {
            Text.Italic("Свина и так зовут \"").Bold(viewModel.NewName).Italic("\" \U0001F914");
            return;
        }

        Text.Bold(viewModel.NewName).Verbatim(" радостно хрюкает, будто подпевая своему новому имени!");
    }
}
