using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.TickGenerators;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class HistoryMessage(ILogger<HistoryMessage> Logger) : BotMessage(Logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var groupId = userContext.Swines.First(s => s.SwineId == swineId).GroupId;

        var swines = userContext.Swines
            .Where(s => s.GroupId == groupId)
            .Where(s => userContext.Feeds.Where(f => f.SwineId == s.SwineId).Any())
            .OrderByDescending(s => s.Weight)
            .Take(10);

        var plot = CreatePlot(showLegend: true);

        foreach (var swine in swines)
        {
            var feeds = userContext.Feeds
                .Where(f => f.SwineId == swine.SwineId)
                .Select(f => new { f.DateTime, f.Amount });

            var losses = userContext.WeightLosses
                .Where(f => f.SwineId == swine.SwineId)
                .Select(f => new { f.DateTime, f.Amount });

            var changes = feeds
                .Concat(losses)
                .OrderBy(wc => wc.DateTime)
                .AsEnumerable()
                .Select(x => new WeightChange(x.DateTime, x.Amount))
                .ToList();

            AddChanges(plot, changes, swine.Name);
        }

        string path = SavePlot(plot);

        PhotoFilePath = path;

        Text.Italic("История веса свинок");

        return Task.CompletedTask;
    }

    private Plot CreatePlot(bool showLegend = false)
    {
        Plot plot = new();

        var axis = plot.Axes.DateTimeTicksBottom();
        var tg = (DateTimeAutomatic)axis.TickGenerator;
        tg.LabelFormatter = dt => dt.ToString("d MMM", Common.RuCulture);

        SetDefaultFont(plot);

        plot.Axes.Margins(0, 0.01, 0, 0.07);

        if (showLegend)
        {
            plot.ShowLegend();
            plot.Legend.Alignment = Alignment.UpperLeft;
            plot.Legend.FontSize = 20;
        }

        return plot;
    }

    private static void AddChanges(Plot plot, IReadOnlyCollection<WeightChange> changes, string caption = null)
    {
        var xValues = changes.Select(c => c.DateTime).ToList();

        var weight = 1;
        var yValues = changes.Select(c =>
        {
            weight += c.Amount;
            return weight;
        }).ToList();

        var sp = plot.Add.Scatter(xValues, yValues);
        sp.LineWidth = 2;
        sp.MarkerSize = 7;

        if (caption is null)
        {
            sp.FillY = true;
            sp.FillYColor = sp.LineColor.WithAlpha(0.5);
        }
        else
        {
            sp.LegendText = caption;
        }
    }

    private static string SavePlot(Plot plot)
    {
        var path = System.IO.Path.GetTempFileName();
        plot.SavePng(path, 1000, 1000);
        return path;
    }

    private void SetDefaultFont(Plot plot)
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto-Regular.ttf");
        if (File.Exists(fontPath) == false)
        {
            Logger.LogError("Default font {path} was not found", fontPath);
            return;
        }

        Fonts.AddFontFile("Roboto", fontPath);

        plot.Axes.Bottom.Label.FontName = "Roboto";
        plot.Axes.Left.Label.FontName = "Roboto";
        plot.Axes.Bottom.TickLabelStyle.FontName = "Roboto";
        plot.Axes.Left.TickLabelStyle.FontName = "Roboto";
        plot.Legend.FontName = "Roboto";
    }

    private record WeightChange(DateTime DateTime, int Amount);
}
