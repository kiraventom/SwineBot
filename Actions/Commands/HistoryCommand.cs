using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.TickGenerators;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/history", "История веса свинок \U0001f4c8")]
public class HistoryCommand(ILogger<HistoryCommand> logger, UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<HistoryMessage, HistoryViewModel>(messageFactory, achievController)
{
    protected override async Task<HistoryViewModel> ExecuteInternal(Update update, string parameter)
    {
        var senderSwine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var swines = await context.Swines
            .Where(s => s.GroupId == senderSwine.GroupId)
            .Where(s => context.Feeds.Any(f => f.SwineId == s.SwineId))
            .OrderByDescending(s => s.Weight)
            .Take(10)
            .ToListAsync();

        var swineIds = swines.Select(s => s.SwineId).ToList();

        var feeds = await context.Feeds
            .Where(f => swineIds.Contains(f.SwineId))
            .Select(f => new { f.SwineId, f.DateTime, f.Amount })
            .ToListAsync();

        var losses = await context.WeightLosses
            .Where(f => swineIds.Contains(f.SwineId))
            .Select(f => new { f.SwineId, f.DateTime, f.Amount })
            .ToListAsync();

        var plot = CreatePlot(showLegend: true);

        foreach (var swine in swines)
        {
            var swineFeeds = feeds.Where(f => f.SwineId == swine.SwineId);
            var swineLosses = losses.Where(l => l.SwineId == swine.SwineId);

            var changes = swineFeeds.Concat(swineLosses)
                .OrderBy(x => x.DateTime)
                .Select(x => new WeightChange(x.DateTime, x.Amount))
                .ToList();

            var swineName = SwineUtils.GetShortName(swine);
            AddChanges(plot, changes, swineName);
        }

        var bytes = await GetPlotBytes(plot);
        return new HistoryViewModel(bytes);
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

    private static async Task<byte[]> GetPlotBytes(Plot plot) => await Task.Run(() => plot.GetImageBytes(1000, 1000, ImageFormat.Png));

    private void SetDefaultFont(Plot plot)
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Roboto-Regular.ttf");
        if (File.Exists(fontPath) == false)
        {
            logger.LogError("Default font {path} was not found", fontPath);
            return;
        }

        Fonts.AddFontFile("Roboto", fontPath);

        plot.Axes.Bottom.Label.FontName = "Roboto";
        plot.Axes.Left.Label.FontName = "Roboto";
        plot.Axes.Bottom.TickLabelStyle.FontName = "Roboto";
        plot.Axes.Left.TickLabelStyle.FontName = "Roboto";
        plot.Legend.FontName = "Roboto";
    }
}
