using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

// Sent only if request was sent from private messages
public class DuelRequestSentMessage : BotMessage<DuelRequestSentViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelRequestSentViewModel viewModel)
    {
        Text.Verbatim("Вызов на дуэль отправлен ").Bold(viewModel.SwineName).Verbatim(".");
    }
}

