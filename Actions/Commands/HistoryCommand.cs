using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.Utils;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/history", "История веса свинок \U0001f4c8")]
public class HistoryCommand(ILogger<HistoryCommand> logger, GraphBuilder graphBuilder, UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<HistoryMessage, HistoryViewModel>(messageFactory, achievController)
{
    protected override async Task<HistoryViewModel> ExecuteInternal(Update update, string parameter)
    {
        var senderSwine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var graphBytes = await graphBuilder.DrawGraph(c => c.Swines
            .Where(s => s.GroupId == senderSwine.GroupId)
            .Where(s => context.Feeds.Any(f => f.SwineId == s.SwineId))
            .OrderByDescending(s => s.Weight)
            .Take(10));

        return new HistoryViewModel(graphBytes);
    }
}
