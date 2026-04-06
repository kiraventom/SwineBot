using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Start;
using SwineBot.BotMessages.Start.Actions;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/piggery", "Осмотреть свинарник \U0001F6D6")]
public class PiggeryCommand(UserContext context, IMessageFactory messageFactory, AchievementController achievController, StartLinkBuilder linkBuilder, Config config) : Command<PiggeryMessage, PiggeryViewModel>(messageFactory, achievController)
{
    protected override async Task<PiggeryViewModel> ExecuteInternal(Update update, string parameter)
    {
        Swine selectedSwine = null;
        string selectedSwineGroupTitle = null;
        if (update.SwineId >= 0)
        {
            selectedSwine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
            selectedSwineGroupTitle = (await context.Groups.FirstAsync(g => g.GroupId == selectedSwine.GroupId)).Title;
        }

        var swinesCount = await context.Swines
            .Where(s => s.OwnerId == update.UserId)
            .CountAsync();

        var swines = await context.Swines
            .Where(s => s.OwnerId == update.UserId)
            .Select(s => new { Swine = s, GroupTitle = context.Groups.First(g => g.GroupId == s.GroupId).Title })
            .ToListAsync();

        var swinesFromGroups = swines
            .Select(s => new SwineFromGroup( 
                s.Swine, 
                s.GroupTitle,
                linkBuilder.Build(new SetSwineAction() { SwineId = s.Swine.SwineId })))
            .OrderByDescending(sg => sg.Swine.Weight);

        return new PiggeryViewModel(swinesCount, swinesFromGroups, selectedSwine, selectedSwineGroupTitle, update.IsPrivateChat, config.Username);
    }
}

