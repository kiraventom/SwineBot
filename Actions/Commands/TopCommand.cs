using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/top", "Топ свинов \U0001f4cb")]
public class TopCommand(UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<TopMessage, TopViewModel>(messageFactory, achievController)
{
    protected override async Task<TopViewModel> ExecuteInternal(Update update, string parameter)
    {
        var senderSwine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var swines = context.Swines
            .Where(s => s.GroupId == senderSwine.GroupId)
            .OrderByDescending(s => s.Weight);

        var topSwines = swines
            .Take(10)
            .Where(s => s.Weight > 1);

        var senderIndex = await swines
            .CountAsync(s => s.Weight > senderSwine.Weight);

        return new TopViewModel(topSwines, senderSwine, senderIndex);
    }
}
