using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class HistoryAllMessage(ILogger logger) : HistoryMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swines = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .Where(s => s.Feeds.Any())
            .OrderByDescending(s => s.Weight)
            .Take(10);

        var plot = CreatePlot(showLegend: true);

        foreach (var swine in swines)
        {
            var feeds = swine.Feeds
                .Select(f => new WeightChange(f.DateTime, f.Amount));

            var losses = swine.WeightLosses
                .Select(f => new WeightChange(f.DateTime, f.Amount));

            var changes = feeds.Concat(losses).OrderBy(wc => wc.DateTime).ToList();
            AddChanges(plot, changes, swine.Name);
        }

        string path = SavePlot(plot);

        PhotoFilePath = path;

        Text.Italic("История веса свинок");

        return Task.CompletedTask;
    }
}

