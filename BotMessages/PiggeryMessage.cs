using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class PiggeryMessage : BotMessage<PiggeryViewModel>
{
    public override void Init<T>(ILogger<T> logger, PiggeryViewModel viewModel)
    {
        if (viewModel.SwinesCount == 0)
        {
            Text.Verbatim("Ваш свинарник пуст :(").LineBreak();
            Text.Verbatim($"Для того, чтобы завести свинку, добавьте бота в группу и отправьте {InfoCommand.COMMAND_NAME}").LineBreak();
            return;
        }

        if (viewModel.SwinesCount == 1)
        {
            if (viewModel.SelectedSwine is null)
            {
                var onlySwine = viewModel.SwinesFromGroups.First().Swine;
                throw new NotSupportedException($"User [{onlySwine.OwnerId}] has one swine [{onlySwine.SwineId}], but it is not selected as private one");
            }

            Text.Verbatim("В вашем свинарнике всего один свин, ").Bold(viewModel.SelectedSwine.Name);

            if (viewModel.IsPrivate)
                Text.Verbatim($" из \"{viewModel.SelectedSwineGroupTitle}\"");

            Text.LineBreak();
            Text.Verbatim("Для увеличения свинарника добавляйте бота в другие группы");
        }

        if (viewModel.SelectedSwine is null && viewModel.IsPrivate)
            Text.Verbatim("Для использования бота в личных сообщениях необходимо выбрать свина:").LineBreak().LineBreak();

        Text.Bold("Ваш свинарник:").LineBreak();

        int index = 0;
        foreach (var swineFromGroup in viewModel.SwinesFromGroups)
        {
            ++index;
            var caption = $"{swineFromGroup.Swine.Name} из \"{swineFromGroup.GroupTitle}\": {swineFromGroup.Swine.Weight} кг";

            Text.Verbatim($"{index}. ");

            if (!viewModel.IsPrivate)
            {
                Text.Verbatim(caption).LineBreak();
                continue;
            }

            if (viewModel.SelectedSwine != null && swineFromGroup.Swine.SwineId == viewModel.SelectedSwine.SwineId)
            {
                Text.Underline(caption);
            }
            else
            {
                Text.InlineUrl(caption, swineFromGroup.SelectStartLink);
            }

            Text.LineBreak();
        }

        if (!viewModel.IsPrivate)
        {
            Text.LineBreak();
            Text.Italic("Отправьте эту команду в ").InlineMention("личном диалоге с ботом", viewModel.BotUsername).Italic(", чтобы кормить свинов там").LineBreak();
        }
    }
}
