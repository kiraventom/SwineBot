using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.BotMessages.Feed;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class InfoMessage : BotMessage<InfoViewModel>
{
    public override void Init<T>(ILogger<T> logger, InfoViewModel viewModel)
    {
        string lastFeedDTStr = GetLastDTStr(viewModel.RecentFeedDTs, viewModel.UtcNow);
        string lastThrowUpDTStr = GetLastDTStr(viewModel.RecentThrowupDTs, viewModel.UtcNow);

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

        // TODO active duel requests (incoming and outcoming)
    }

    private static string GetLastDTStr(IReadOnlyCollection<DateTime> recentDTs, DateTime current)
    {
        if (recentDTs.Count == 0)
            return "так давно, что никогда...";

        var lastDT = recentDTs.Max();
        var diff = current - lastDT;
        if (diff.TotalMinutes < 1)
        {
            return "Только что";
        }
        else if (diff.TotalHours < 1)
        {
            var totalMin = (int)diff.TotalMinutes;
            var minutesDecl = MessageTextUtils.GetDeclinatedNoun(totalMin, Unit.Minute);
            return $"{totalMin} {minutesDecl} назад";
        }
        else
        {
            var totalHours = (int)diff.TotalHours;
            var hoursDecl = MessageTextUtils.GetDeclinatedNoun(totalHours, Unit.Hour);
            return $"{totalHours} {hoursDecl} назад";
        }
    }
}



