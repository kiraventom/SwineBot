using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Senders;
using SwineBot.Text;

namespace SwineBot.Actions.Commands.Duel;

[CommandInfo(HANDLE, "Отменить вызов на дуэль", Hidden = true)]
public class DuelCancelCommand(ILogger<DuelCancelCommand> logger, IMessageEditor messageEditor, UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<DuelCancelMessage, DuelCancelViewModel>(messageFactory, achievController)
{
    public const string HANDLE = "/duelcancel";

    public async Task ExecuteSilent(Update update)
    {
        await ExecuteInternal(update, null);
    }

    protected override async Task<DuelCancelViewModel> ExecuteInternal(Update update, string parameter)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var duelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.AttackerId == swine.SwineId);
        if (duelRequest is null)
            return new DuelCancelViewModel(HadActiveDuel: false, OpponentName: null);

        var opponent = await context.Swines.FirstAsync(s => s.SwineId == duelRequest.DefenderId);

        context.DuelRequests.Remove(duelRequest);

        if (duelRequest.MessageId is {} messageId)
        {
            var group = await context.Groups.FirstAsync(g => g.GroupId == swine.GroupId);
            var tgGroupId = group.TelegramId;

            try
            {
                await messageEditor.RemoveReplyMarkup(tgGroupId, messageId);
                
                var messageText = new MessageText();
                messageText.Verbatim("Вызов от ").Bold(swine.Name).Verbatim(" был отменён");
                await messageEditor.EditText(tgGroupId, messageId, messageText.ToString());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to edit message {messageId} in chat {chatId}", tgGroupId, messageId);
            }
        }
        else
        {
            logger.LogError("Failed to edit duel request message, MessageId is null");
        }

        return new DuelCancelViewModel(HadActiveDuel: true, OpponentName: opponent.Name);
    }
}



