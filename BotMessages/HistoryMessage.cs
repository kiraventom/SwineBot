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

        var plot = CreatePlot();
        AddChanges(plot, changes);
        string path = SavePlot(plot);

        PhotoFilePath = path;

        Text.Italic("История веса ").Bold(swine.Name);

        return Task.CompletedTask;
    }

    protected static Plot CreatePlot(bool showLegend = false)
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

    protected static void AddChanges(Plot plot, IReadOnlyCollection<WeightChange> changes, string caption = null)
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

    protected static string SavePlot(Plot plot)
    {
        var path = System.IO.Path.GetTempFileName();
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
        plot.Legend.FontName = "Roboto";
    }

    protected record WeightChange(DateTime DateTime, int Amount);
}
