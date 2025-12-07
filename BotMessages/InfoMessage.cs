using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class InfoMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines
            .Include(s => s.Stats)
            .Include(s => s.Feeds)
            .FirstOrDefault(s => s.OwnerId == userId);

        var owner = userContext.Users.First(u => u.UserId == userId);

        var duels = userContext.DuelResults.ToList();
        var wonDuels = duels.Count(d => d.WinnerId == swine.SwineId);
        var lostDuels = duels.Count(d => d.LoserId == swine.SwineId);

        var current = DateTime.Now;
        var recentFeeds = swine.Feeds.Where(f => (current - f.DateTime).TotalHours < 24).ToList();
        string lastFeedDTStr = GetLastFeedStr(recentFeeds, current);

        Text.Bold("Информация о свине ").InlineMention(owner).Bold(":").LineBreak()
            .LineBreak()
            .Italic("Имя: ").Verbatim(swine.Name).LineBreak()
            .Italic("Вес: ").Verbatim($"{swine.Weight} кг").LineBreak()
            .Italic("Приёмы пищи (за 24 ч): ").Verbatim(recentFeeds.Count.ToString()).Verbatim("; последний: ").Verbatim(lastFeedDTStr).LineBreak()
            .Italic("Статистика дуэлей: ").Verbatim(wonDuels.ToString()).Verbatim(" побед, ").Verbatim(lostDuels.ToString()).Verbatim(" поражений");

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
            var minutesDecl = MessageTextUtils.GetDeclinatedNoun(totalMin, "минута", "минуты", "минут");
            return $"{totalMin} {minutesDecl} назад";
        }
        else
        {
            var totalHours = (int)diff.TotalHours;
            var hoursDecl = MessageTextUtils.GetDeclinatedNoun(totalHours, "час", "часа", "часов");
            return $"{totalHours} {hoursDecl} назад";
        }
    }
}

