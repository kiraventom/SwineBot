using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Actions.Commands.Duel;
using SwineBot.BotMessages.Feed;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class InfoMessage : BotMessage<InfoViewModel>
{
    public override void Init<T>(ILogger<T> logger, InfoViewModel viewModel)
    {
        string lastFeedDTStr = MessageTextUtils.GetLastDTStr(viewModel.RecentFeedDTs, viewModel.UtcNow);
        string lastThrowUpDTStr = MessageTextUtils.GetLastDTStr(viewModel.RecentThrowupDTs, viewModel.UtcNow);

        Text.Bold("Информация о свине ").InlineMention(viewModel.Sender).Bold(":").LineBreak()
            .LineBreak()
            .Italic("Имя: ").Verbatim(viewModel.SenderSwine.Name).LineBreak()
            .Italic("Вес: ").Verbatim($"{viewModel.SenderSwine.Weight} кг").LineBreak();

        var mealsDecl = MessageTextUtils.GetDeclinatedNoun(viewModel.RecentFeedDTs.Count, Unit.Meal);
        mealsDecl = char.ToUpper(mealsDecl[0]) + mealsDecl[1..];

        Text.Italic(mealsDecl)
           .Italic($" пищи (за {FeedGenerator.OVERFEED_COOLDOWN} ч): ").Verbatim(viewModel.RecentFeedDTs.Count).Verbatim("; последний: ").Verbatim(lastFeedDTStr).LineBreak();

        if (viewModel.RecentThrowupDTs.Count != 0)
            Text.Italic("Неудачный перекорм: ").Verbatim(lastThrowUpDTStr).LineBreak();

        if (viewModel.ConsecutiveOverfeeds != 0)
        {
            Text.Italic("Перекормов: ").Verbatim(viewModel.ConsecutiveOverfeeds).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(viewModel.ConsecutiveOverfeeds, Unit.Time))
                .Verbatim(" подряд").LineBreak();
        }

        if (viewModel.ConsecutiveNoOverfeeds != 0)
        {
            Text.Italic("Без перекормов: ").Verbatim(viewModel.ConsecutiveNoOverfeeds).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(viewModel.ConsecutiveNoOverfeeds, Unit.Time))
                .Verbatim(" подряд").LineBreak();
        }

        if (viewModel.WonDuels != 0 || viewModel.LostDuels != 0)
        {
            Text.Italic("Статистика дуэлей: ").Verbatim(viewModel.LostDuels).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(viewModel.LostDuels, Unit.Win))
                .Verbatim(", ")
                .Verbatim(viewModel.LostDuels).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(viewModel.LostDuels, Unit.Loss))
                .LineBreak();
        }

        if (viewModel.SlaughtersCount > 0)
        {
            Text.Italic("Предшественников погибло: ").Verbatim(viewModel.SlaughtersCount);
            if (viewModel.GrowthPercent > 0)
            {
                Text.Verbatim("; рост ускорен на ")
                    .Verbatim(viewModel.GrowthPercent)
                    .Verbatim("%");
            }

            Text.LineBreak();
        }

        if (viewModel.OutcomingDuelTargetName is not null)
        {
            Text.Italic("Отправлен вызов на дуэль ").Bold(viewModel.OutcomingDuelTargetName);
            Text.Verbatim(" (отправьте ").Verbatim(DuelCancelCommand.HANDLE).Verbatim(" для отмены)").LineBreak();
        }

        if (viewModel.IncomingDuelSourceName is not null)
        {
            Text.Italic("Получен вызов на дуэль от ").Bold(viewModel.IncomingDuelSourceName).LineBreak();
        }
    }
}



