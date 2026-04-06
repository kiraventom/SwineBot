using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Actions.Commands;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class SlaughterMessage : BotMessage<SlaughterViewModel>
{
    public override void Init<T>(ILogger<T> logger, SlaughterViewModel viewModel)
    {
        var swine = viewModel.SenderSwine;

        if (viewModel.IsTooEarlySlaughter)
        {
            Text.Italic("Нельзя марать руки в крови так часто.");
            return;
        }

        var achievsCount = viewModel.AchievsCount;

        if (viewModel.ReceivedConfirmation)
        {
            Text.Bold(swine.Name).Italic(" жалобно визжит и испускает последний вздох.").LineBreak();
            if (viewModel.WasSlaughterEffective.Value)
                Text.Italic("Теперь ваши будущие свинки будут расти быстрее...");
            else
                Text.Italic("Это жестокое убийство не принесло никакого эффекта.");
            return;
        }

        Text.Italic("Вы собираетесь").Verbatim(" ").Underline("убить").Verbatim(" ").Italic("вашу свинку ").Bold(swine.Name).LineBreak();
        Text.Italic("Вы потеряете ").Bold(swine.Weight).Italic($" {MessageTextUtils.GetDeclinatedNoun(swine.Weight, Unit.Kg)} сальца");
        if (achievsCount != 0)
            Text.Italic(", ").Bold(achievsCount).Italic($" {MessageTextUtils.GetDeclinatedNoun(achievsCount, Unit.Achievement)}");

        Text.Italic(" и верного друга.").LineBreak().LineBreak();

        Text.Bold(swine.Name).Verbatim(" радостно смотрит на вас, думая, что вы принесли корм.").LineBreak();
        Text.Italic("Чтобы убить ").Bold(swine.Name).Italic(", отправьте ")
           .Monospace($"{SlaughterCommand.COMMAND_NAME} {SlaughterCommand.CONFIRMATION}");
    }
}
