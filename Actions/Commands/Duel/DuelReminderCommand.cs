using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo("/duelremind", "service command", Type = CommandType.Service)]
public class DuelReminderCommand(ILogger<DuelReminderCommand> logger, UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<DuelReminderMessage, DuelReminderViewModel>(messageFactory, achievController)
{
    public static async Task<string> BuildLink(UserContext context, Update update)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var duelRequest = await context.DuelRequests.FirstAsync(dr => dr.DefenderId == update.SwineId);
        var group = await context.Groups.FirstAsync(g => g.GroupId == swine.GroupId);
        var groupTelegramIdStr = group.TelegramId.ToString();

        if (groupTelegramIdStr.StartsWith("-100"))
            groupTelegramIdStr = groupTelegramIdStr[4..];
        else
            return null;

        return $"https://t.me/c/{groupTelegramIdStr}/{duelRequest.MessageId}";
    }

    protected override async Task<DuelReminderViewModel> ExecuteInternal(Update update, string parameter)
    {
        var link = await BuildLink(context, update);
        var duelRequest = await context.DuelRequests.FirstAsync(dr => dr.DefenderId == update.SwineId);
        return new DuelReminderViewModel(link, duelRequest.DateTime);
    }
}
