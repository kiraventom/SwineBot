using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class SelectDuelOpponentMessage : BotMessage<SelectDuelOpponentViewModel>
{
    public override bool Notify => false;

    public override void Init<TMessage>(ILogger<TMessage> logger, SelectDuelOpponentViewModel viewModel)
    {
        if (viewModel.OpponentsCount == 0)
        {
            Text.Verbatim("В группе нет подходящих оппонентов :(");
            return;
        }

        if (viewModel.CurrentRequestOpponent is not null)
        {
            Text.Bold(viewModel.CurrentRequestOpponent).Verbatim(" ещё не ответил на ваш вызов.").LineBreak()
                .Underline("Этот вызов отменится, если вы отправите другой.").LineBreak().LineBreak();
        }

        Text.Verbatim("Вы отправляете вызов на дуэль в группу ").Bold(viewModel.GroupName).Verbatim("!").LineBreak();
        Keyboard.AddSwitchInlineQueryCurrentChat("Выбрать оппонента", viewModel.Query);
    }
}
