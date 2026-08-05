using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class DuelCancelMessage : BotMessage<DuelCancelViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelCancelViewModel viewModel)
    {
        if (!viewModel.HadActiveDuel)
        {
            Text.Verbatim("Вы не отправляли вызовов на дуэль");
            return;
        }

        Text.Verbatim("Ваш вызов ").Bold(viewModel.OpponentName).Verbatim(" отменён");
    }
}

