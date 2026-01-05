using Microsoft.EntityFrameworkCore;
using ScottPlot;
using ScottPlot.TickGenerators;
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

        string path = GeneratePlotPng(changes);

        PhotoFilePath = path;

        Text.Italic("История веса ").Bold(swine.Name);

        return Task.CompletedTask;
    }

    private static string GeneratePlotPng(IReadOnlyCollection<WeightChange> changes)
    {
        var xValues = changes.Select(c => c.DateTime).ToList();

        var weight = 1;
        var yValues = changes.Select(c =>
        {
            weight += c.Amount;
            return weight;
        }).ToList();

        var path = System.IO.Path.GetTempFileName();

        ScottPlot.Plot plot = new();

        var sp = plot.Add.Scatter(xValues, yValues);
        sp.LineWidth = 2;
        sp.MarkerSize = 7;
        sp.FillY = true;
        sp.FillYColor = sp.LineColor.WithAlpha(0.2);

        plot.Axes.Margins(0, 0.01, 0, 0.07);

        var axis = plot.Axes.DateTimeTicksBottom();
        var tg = (DateTimeAutomatic)axis.TickGenerator;
        tg.LabelFormatter = dt => dt.ToString("d MMM", Common.RuCulture);

        SetDefaultFont(plot);

        plot.SavePng(path, 1000, 1000);
        return path;
    }

    private static void SetDefaultFont(Plot plot)
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto-Regular.ttf");
        if (File.Exists(fontPath) == false)
        {
            Log.Error("Default font {path} was not found", fontPath);
            return;
        }

        Fonts.AddFontFile("Roboto", fontPath);

        plot.Axes.Bottom.Label.FontName = "Roboto";
        plot.Axes.Left.Label.FontName = "Roboto";
        plot.Axes.Bottom.TickLabelStyle.FontName = "Roboto";
        plot.Axes.Left.TickLabelStyle.FontName = "Roboto";
    }

    private record WeightChange(DateTime DateTime, int Amount);
}
