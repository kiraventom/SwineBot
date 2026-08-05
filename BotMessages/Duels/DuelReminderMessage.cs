using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class DuelReminderMessage : BotMessage<DuelReminderViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelReminderViewModel viewModel)
    {
        var dateTimeStr = viewModel.RequestDateTime.ToString("m", Common.RuCulture);

        if (viewModel.IsNotSupergroup)
        {
            Text.Verbatim("Вас вызвали на дуэль!").LineBreak();
            Text.Verbatim("Дата вызова: ").Monospace(dateTimeStr).LineBreak();
            Text.Verbatim("(Чтобы здесь появилась ссылка на вызов, ")
                .InlineUrl("преобразуйте группу в супергруппу", @"https://telegra.ph/Kak-preobrazovat-gruppu-v-supergruppu-08-05")
                .Verbatim(")")
                .LineBreak().LineBreak();
        }
        else
        {
            Text.Verbatim("Вас вызвали на ").InlineUrl("дуэль!", viewModel.LinkToMessage).LineBreak();
            Text.Verbatim("Дата вызова: ").Monospace(dateTimeStr).LineBreak().LineBreak();
        }

        Text.Italic("Примите или отклоните вызов, чтобы вернуться к управлению свином");
    }
}

