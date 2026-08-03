using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class SelectDuelOpponentMessage : BotMessage<SelectDuelOpponentViewModel>
{
    public override bool Notify => false;

    public override void Init<TMessage>(ILogger<TMessage> logger, SelectDuelOpponentViewModel viewModel)
    {
        if (viewModel.IsPrivate)
        {
            Text.Verbatim("Оппонент получит вызов в личные сообщения.").LineBreak().LineBreak();
            Text.Italic("Выберите оппонента для дуэли:");
        }
        else
        {
            Text.Verbatim("Вы отправляете публичный вызов на дуэль!").LineBreak().LineBreak();
            Text.Italic("Выберите оппонента для дуэли:");
        }

        Keyboard.SetSwitchInlineQueryCurrentChat("Выбрать оппонента", viewModel.GroupId);
    }
}

