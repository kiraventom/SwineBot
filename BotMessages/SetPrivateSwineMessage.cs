using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class SetPrivateSwineMessage : BotMessage<SetPrivateSwineViewModel>, IPinnableMessage
{
    public bool ShouldPin => true;

    public override void Init<T>(ILogger<T> logger, SetPrivateSwineViewModel viewModel)
    {
        Text.Verbatim("Выбранный свин: ").Bold(viewModel.SwineName).Verbatim($" из \"{viewModel.GroupTitle}\"");
    }
}

