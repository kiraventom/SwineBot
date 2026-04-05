using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class InfoMessage(ILogger<InfoMessage> logger, UserContext context, IDateTimeNowProvider dtnProvider) : BotMessage(logger)
{
    protected override async Task InitInternal(Update update)
    {
        var swine = context.Swines.First(s => s.SwineId == update.SwineId);

        var duels = await context.DuelResults.ToListAsync();
        var wonDuels = duels.Count(d => d.WinnerId == update.SwineId);
        var lostDuels = duels.Count(d => d.LoserId == update.SwineId);

        var current = dtnProvider.UtcNow;

        var recentFeeds = await context.GetRecentFeeds(update.SwineId, current);
        var recentThrowups = await context.GetRecentThrowups(update.SwineId, current);

        string lastFeedDTStr = GetLastDTStr(recentFeeds.Select(f => f.DateTime).ToList(), current);
        string lastThrowUpDTStr = GetLastDTStr(recentThrowups.Select(f => f.DateTime).ToList(), current);

        int consecutiveOverfeeds = await OverfeedAchievementChecker.CountConsecutiveOverfeeds(context, update.SwineId);
        int consecutiveNoOverfeeds = await NoOverfeedAchievementChecker.CountConsecutiveNoOverfeeds(context, update.SwineId);

        var owner = context.Users.First(u => u.UserId == swine.OwnerId);
        Text.Bold("Информация о свине ").InlineMention(owner).Bold(":").LineBreak()
            .LineBreak()
            .Italic("Имя: ").Verbatim(swine.Name).LineBreak()
            .Italic("Вес: ").Verbatim($"{swine.Weight} кг").LineBreak();

        var mealsDecl = MessageTextUtils.GetDeclinatedNoun(recentFeeds.Count, Unit.Meal);
        mealsDecl = char.ToUpper(mealsDecl[0]) + mealsDecl[1..];

        logger.LogInformation(mealsDecl);

        Text.Italic(mealsDecl)
           .Italic($" пищи (за {FeedGenerator.OVERFEED_COOLDOWN} ч): ").Verbatim(recentFeeds.Count).Verbatim("; последний: ").Verbatim(lastFeedDTStr).LineBreak();

        if (recentThrowups.Count != 0)
        {
            Text.Italic("Неудачный перекорм: ").Verbatim(lastThrowUpDTStr).LineBreak();
        }

        if (consecutiveOverfeeds != 0)
        {
            Text.Italic("Перекормов: ").Verbatim(consecutiveOverfeeds).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(consecutiveOverfeeds, Unit.Time))
                .Verbatim(" подряд").LineBreak();
        }

        if (consecutiveNoOverfeeds != 0)
        {
            Text.Italic("Без перекормов: ").Verbatim(consecutiveNoOverfeeds).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(consecutiveNoOverfeeds, Unit.Time))
                .Verbatim(" подряд").LineBreak();
        }

        if (wonDuels != 0 || lostDuels != 0)
        {
            Text.Italic("Статистика дуэлей: ").Verbatim(wonDuels).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(wonDuels, Unit.Win))
                .Verbatim(", ")
                .Verbatim(lostDuels).Verbatim(" ")
                .Verbatim(MessageTextUtils.GetDeclinatedNoun(lostDuels, Unit.Loss))
                .LineBreak();
        }

        var slaughters = await context.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .Where(s => s.GroupId == swine.GroupId)
            .ToListAsync();

        var totalSlaughteredWeight = slaughters
            .Where(s => s.SwineWeight >= SlaughterMessage.MIN_SWINE_WEIGHT)
            .Sum(s => s.SwineWeight);

        var growthMod = User.GetGrowthModifier(totalSlaughteredWeight);

        if (slaughters.Count > 0)
        {
            Text.Italic("Предшественников погибло: ").Verbatim(slaughters.Count);
            if (growthMod > 1)
            {
                Text.Verbatim("; рост ускорен на ")
                    .Verbatim(((growthMod - 1) * 100).ToString("##"))
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



