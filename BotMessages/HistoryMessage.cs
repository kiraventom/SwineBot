using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class HistoryMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.OwnerId == userId);

        var feeds = swine.Feeds
            .Select(f => new WeightChange(f.DateTime, f.Amount));

        var losses = swine.WeightLosses
            .Select(f => new WeightChange(f.DateTime, f.Amount));

        var changes = feeds.Concat(losses).OrderBy(wc => wc.DateTime).ToList();

        var xValues = changes.Select(c => c.DateTime).ToList();

        var weight = 1;
        var yValues = changes.Select(c => 
        {
            weight += c.Amount;
            return weight;
        }).ToList();

        var path = System.IO.Path.GetTempFileName();

        ScottPlot.Plot plot = new();
        plot.Add.Scatter(xValues, yValues);
        plot.SavePng(path, 400, 400);

        PhotoFilePath = path;

        Text.Italic("История веса ").Bold(swine.Name);

        return Task.CompletedTask;
    }

    private record WeightChange(DateTime DateTime, int Amount);
}
