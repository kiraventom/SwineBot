using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.Actions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public record Update(string MessageText, int? GroupId, int UserId, int? SwineId, bool IsPrivateChat);

public interface IUpdateHandler
{
    Task Handle(Telegram.Bot.Types.Update update, CancellationToken token);
}

public class UpdateHandler(ILogger<UpdateHandler> logger, UserContext context, UserContextHelpers contextHelpers, Config config, IBotMessageSender sender, IEnumerable<UserAction> actions) : IUpdateHandler
{
    private IReadOnlyCollection<UserAction> Actions { get; } = actions.ToList();

    public async Task Handle(Telegram.Bot.Types.Update update, CancellationToken token)
    {
        logger.LogInformation("Received update: {updateType}", update.Type);

        using var transaction = await context.Database.BeginTransactionAsync(token);
        try
        {
            if (update.Message is not { } message)
                return;

            var didHandle = await HandleMessageAsync(message);

            if (didHandle)
                await context.SaveChangesAsync(token);

            await transaction.CommitAsync(token);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(token);
            logger.LogError(e, "Transaction failed, rolling back");
        }
    }

    private async Task<bool> HandleMessageAsync(Message message)
    {
        var sender = message.From;
        var chat = message.Chat;

        if (sender is null)
        {
            logger.LogWarning("Sender is null");
            return false;
        }

        // Bot received its own message (e.g., pinned message notification)
        if (sender.Username == config.Username)
            return false;

        var userId = await contextHelpers.GetOrAddUser(chat.Id, chat.Title, sender.Id, sender.FirstName, sender.Username);
        var groupId = context.Groups.FirstOrDefault(g => g.TelegramId == chat.Id)?.GroupId;
        var isPrivateChat = groupId == null;
        var swineId = await contextHelpers.GetOrSetSwine(groupId, userId);

        var update = new Update(message.Text, groupId, userId, swineId, isPrivateChat);
        LogUpdate(update);

        var botCommand = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (botCommand is not null)
            return await HandleBotCommandAsync(update, botCommand);

        return false;
    }

    private void LogUpdate(Update update)
    {
        if (update.IsPrivateChat)
        {
            logger.LogInformation("Received private message with text '{text}' from user [{userId}]", update.MessageText, update.UserId);
        }
        else
        {
            logger.LogInformation("Received message with text '{text}' in group [{groupId}] from user [{userId}]", update.MessageText, update.GroupId, update.UserId);
        }
    }

    private async Task<bool> HandleBotCommandAsync(Update update, MessageEntity botCommand)
    {
        var commandText = update.MessageText.Substring(botCommand.Offset, botCommand.Length);
        return await HandleUserActionAsync(update, commandText);
    }

    private async Task<bool> HandleUserActionAsync(Update update, string actionText)
    {
        var action = Actions.FirstOrDefault(c => c.IsMatch(actionText));
        if (action is null)
        {
            logger.LogWarning("Action '{actionText}' does not match any of actions: [ {actions} ]", actionText, string.Join(", ", Actions.Select(c => c.Name)));
            return false;
        }

        var botMessage = action.Execute(update, actionText);
        await sender.Send(update, botMessage);
        return true;
    }
}


