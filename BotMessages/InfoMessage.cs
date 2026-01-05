using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class InfoMessage(ILogger logger, AchievementController achievController) : BotMessage(logger)
{
    private const string DOT = "⋅ ";

    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
            .Include(s => s.Feeds)
            .FirstOrDefault(s => s.OwnerId == userId);

        var owner = userContext.Users.First(u => u.UserId == userId);

        var duels = userContext.DuelResults.ToList();
        var wonDuels = duels.Count(d => d.WinnerId == swine.SwineId);
        var lostDuels = duels.Count(d => d.LoserId == swine.SwineId);

        var current = DateTime.Now.ToUniversalTime();
        var recentFeeds = swine.Feeds.Where(f => (current - f.DateTime).TotalHours < 24).ToList();
        string lastFeedDTStr = GetLastFeedStr(recentFeeds, current);

        int consecutiveOverfeeds = OverfeedAchievementChecker.CountConsecutiveOverfeeds(userContext, swine.SwineId);
        int consecutiveNoOverfeeds = NoOverfeedAchievementChecker.CountConsecutiveNoOverfeeds(userContext, swine.SwineId);

        Text.Bold("Информация о свине ").InlineMention(owner).Bold(":").LineBreak()
            .LineBreak()
            .Italic("Имя: ").Verbatim(swine.Name).LineBreak()
            .Italic("Вес: ").Verbatim($"{swine.Weight} кг").LineBreak();

        var mealsDecl = MessageTextUtils.GetDeclinatedNoun(recentFeeds.Count, Unit.Meal);
        mealsDecl = char.ToUpper(mealsDecl[0]) + mealsDecl[1..];

        Text.Italic(mealsDecl)
           .Italic(" пищи (за 24 ч): ").Verbatim(recentFeeds.Count).Verbatim("; последний: ").Verbatim(lastFeedDTStr).LineBreak();

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

        if (swine.Stats.Achievements.Count != 0)
        {
            Text
                .Italic("Достижения: ").LineBreak()
                .Tab(text =>
                {
                    foreach (var achiev in swine.Stats.Achievements.OrderByDescending(a => a.DateTime))
                    {
                        var level = achievController.GetLevel(achiev);
                        if (level is null)
                        {
                            Logger.Error("Level for achievement {type} with value {value} was not found", achiev.Type.ToString(), achiev.Value);
                            continue;
                        }

                        text.Verbatim(DOT).Bold(level.Name)
                            .Verbatim(" (").Verbatim(level.Description).Verbatim(") получено ")
                            .Verbatim(achiev.DateTime.ToString("d MMMM yyyy", Common.RuCulture))
                            .LineBreak();
                    }
                });
        }

        // TODO active duel requests (incoming and outcoming)

        return Task.CompletedTask;
    }

    private static string GetLastFeedStr(IReadOnlyCollection<Feed> recentFeeds, DateTime current)
    {
        if (recentFeeds.Count == 0)
            return "так давно, что никогда...";

        var lastFeedDT = recentFeeds.Max(f => f.DateTime);
        var diff = current - lastFeedDT;
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



