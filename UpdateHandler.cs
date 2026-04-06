using Microsoft.Extensions.Logging;
using SwineBot.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages;

namespace SwineBot;

public record UpdateReply(Update Update, IReadOnlyCollection<IBotMessage> Messages);
public record Update(string MessageText, int? GroupId, int UserId, int? SwineId, long TelegramChatId, bool IsPrivateChat);

public interface IUpdateHandler
{
    Task Handle(Telegram.Bot.Types.Update update, CancellationToken token);
}

public class UpdateHandler(ILogger<UpdateHandler> logger, UserContext context, UserContextHelpers contextHelpers, Config config, IBotMessageSender sender, ICommandFactory commandFactory) : IUpdateHandler
{
    public async Task Handle(Telegram.Bot.Types.Update tgUpdate, CancellationToken token)
    {
        logger.LogInformation("Received update: {updateType}", tgUpdate.Type);

        if (tgUpdate.Message is not { } tgMessage)
            return;

        var botCommand = GetBotCommand(tgMessage);
        if (botCommand is null)
            return;

        var updateReply = await GetUpdateReply(tgMessage, botCommand, token);
        if (updateReply is null)
            return;

        await ReplyToUpdate(updateReply);
    }

    private async Task ReplyToUpdate(UpdateReply updateReply)
    {
        try
        {
            foreach (var message in updateReply.Messages)
            {
                await sender.Send(updateReply.Update, message);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Sending message failed");
        }
    }

    private async Task<UpdateReply> GetUpdateReply(Message message, MessageEntity botCommand, CancellationToken token)
    {
        Update update;
        IReadOnlyCollection<IBotMessage> messages;

        var transaction = await context.Database.BeginTransactionAsync(token);

        try
        {
            update = await CreateUpdate(message.From, message.Chat, message.Text);
            messages = await GetMessages(update, botCommand);

            await context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(token);
            logger.LogError(e, "Transaction failed, rolling back");
            return null;
        }

        return new UpdateReply(update, messages);
    }

    private async Task<Update> CreateUpdate(Telegram.Bot.Types.User tgUser, Chat tgChat, string text)
    {
        if (tgUser is null)
        {
            logger.LogWarning("Sender is null");
            return null;
        }

        // Bot received its own message (e.g., pinned message notification)
        if (tgUser.Username == config.Username)
            return null;

        var senderInfo = await contextHelpers.GetOrAddUser(tgChat.Id, tgChat.Title, tgUser.Id, tgUser.FirstName, tgUser.Username);
        var isPrivateChat = senderInfo.GroupId == null;
        var swineId = await contextHelpers.GetOrSetSwine(senderInfo);

        var update = new Update(text, senderInfo.GroupId, senderInfo.UserId, swineId, tgChat.Id, isPrivateChat);
        LogUpdate(update);

        return update;
    }

    private static MessageEntity GetBotCommand(Message message) => message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

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

    private async Task<IReadOnlyCollection<IBotMessage>> GetMessages(Update update, MessageEntity botCommand)
    {
        var command = GetCommand(update, botCommand, out var parameter);
        if (command is null)
            return [];

        return await command.Execute(update, parameter);
    }

    private ICommand GetCommand(Update update, MessageEntity botCommand, out string parameter)
    {
        parameter = null;

        var commandText = update.MessageText.Substring(botCommand.Offset, botCommand.Length);

        var command = commandFactory.Create(commandText);;
        if (command is null)
            return null;

        parameter = update.MessageText.Substring(botCommand.Offset + botCommand.Length).Trim();

        // SwineId is null -> command in private chat and no private swine is selected -> prompt user to select private swine via PiggeryCommand
        if (update.SwineId is null && command is not StartCommand and not PiggeryCommand)
        {
            command = commandFactory.Create<PiggeryCommand>();
            parameter = string.Empty;
        }

        return command;
    }
}
