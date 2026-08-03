using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo("/duel", "Вызвать на дуэль \u2694\uFE0F")]
public class DuelCommand(UserContext context, IMessageFactory messageFactory, AchievementController achievController)
    : Command<SelectDuelOpponentMessage, SelectDuelOpponentViewModel>(messageFactory, achievController)
{
    protected override async Task<SelectDuelOpponentViewModel> ExecuteInternal(Update update, string parameter)
    {
        if (!string.IsNullOrEmpty(parameter) && long.TryParse(parameter, out var opponentTgId))
        {
            var opponent = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == opponentTgId);
            if (opponent is null)
                throw new NotSupportedException($"User with TelegramId={opponentTgId} not found");

            var opponentSwine = await context.Swines
                .Where(s => s.OwnerId == opponent.UserId)
                .FirstOrDefaultAsync(s => s.GroupId == update.GroupId);

            if (opponentSwine is null)
                throw new NotSupportedException($"User {opponent.UserId} does not have swines in the group {update.GroupId}");

            // TODO return other view model?
            return null;
        }

        var user = await context.Users.FirstAsync(u => u.UserId == update.UserId);
        var isPrivate = update.IsPrivateChat;
        int groupId;

        if (isPrivate)
        {
            var privateSwineId = user.PrivateSwineId;
            var privateSwine = await context.Swines.FirstAsync(s => s.SwineId == privateSwineId);
            groupId = privateSwine.GroupId.Value;
        }
        else
        {
            groupId = update.GroupId.Value;
        }

        var group = await context.Groups.FirstAsync(g => g.GroupId == groupId);

        var viewModel = new SelectDuelOpponentViewModel(group.TelegramId.ToString(), isPrivate);
        return viewModel;
    }
}


