using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class InvalidMessage : BotMessage<InvalidViewModel>
{
    public override void Init<T>(ILogger<T> logger, InvalidViewModel viewModel)
    {
        Text.Bold("Что-то пошло не так :(");
    }
}
