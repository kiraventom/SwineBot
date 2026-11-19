using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using SwineBot.Achievements;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo("/duelremind", "service command", Type = CommandType.Service)]
public class DuelReminderCommand(UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<DuelReminderMessage, DuelReminderViewModel>(messageFactory, achievController)
{
    /// <summary>
    /// Only supergroups have the <c>t.me/c/</c> message links, i.e. negative ids with the -100 prefix.
    /// </summary>
    public static bool IsSupergroup(long groupTelegramId) => groupTelegramId.ToString().StartsWith("-100");

    public static string BuildLink(long groupTelegramId, int? messageId)
    {
        if (messageId is not { } id || !IsSupergroup(groupTelegramId))
            return null;

        var groupTelegramIdStr = groupTelegramId.ToString();
        return $"https://t.me/c/{groupTelegramIdStr[4..]}/{id}";
    }

    protected override async Task<DuelReminderViewModel> ExecuteInternal(Update update, string parameter)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var duelRequest = await context.DuelRequests.FirstAsync(dr => dr.DefenderId == update.SwineId);
        var group = swine.GroupId is { } groupId
            ? await context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId)
            : null;
        var isSupergroup = group is not null && IsSupergroup(group.TelegramId);

        var link = isSupergroup ? BuildLink(group.TelegramId, duelRequest.MessageId) : null;

        return new DuelReminderViewModel(link, duelRequest.DateTime, IsNotSupergroup: !isSupergroup);
    }
}
