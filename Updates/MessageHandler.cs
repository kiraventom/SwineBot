using Microsoft.Extensions.Logging;
using SwineBot.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages;
using SwineBot.Senders;

namespace SwineBot.Updates;

public class MessageHandler(ILogger<MessageHandler> logger, UserContext context, UserContextHelpers contextHelpers, Config config, IBotMessageSender sender, ICommandFactory commandFactory) : UpdateTypeHandler<Telegram.Bot.Types.Message>
{
    public override async Task<UpdateHandleResult> Handle(Message message, CancellationToken token)
    {
        if (message.ForwardOrigin is not null)
            return UpdateHandleResult.MessageForward;

        var botCommand = GetBotCommand(message);
        if (botCommand is null)
        {
            if (message.Text is {} text)
                logger.LogInformation("Not command: {text}", text);

            return UpdateHandleResult.MessageNotCommand;
        }

        var updateReply = await GetUpdateReply(message, botCommand, token);
        if (updateReply is null)
            return UpdateHandleResult.DatabaseFail;

        var didReply = await SendUpdateReply(updateReply);
        if (!didReply)
            return UpdateHandleResult.SendMessageFail;

        return UpdateHandleResult.MessageOK;
    }

    private void LogUpdate(Update update)
    {
        if (update.IsPrivateChat)
        {
            logger.LogInformation("Received private message with text '{text}' from user [{userId}]", update.Text, update.UserId);
        }
        else
        {
            logger.LogInformation("Received message with text '{text}' in group [{groupId}] from user [{userId}]", update.Text, update.GroupId, update.UserId);
        }
    }

    private async Task<Update> CreateUpdate(Message tgMessage)
    {
        var tgUser = tgMessage.From;
        var tgChat = tgMessage.Chat;
        var text = tgMessage.Text;

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

    private async Task<UpdateReply> GetUpdateReply(Message message, MessageEntity botCommand, CancellationToken token)
    {
        Update update;
        IReadOnlyCollection<IBotMessage> messages;

        var transaction = await context.Database.BeginTransactionAsync(token);

        try
        {
            update = await CreateUpdate(message);
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

    private async Task<bool> SendUpdateReply(UpdateReply updateReply)
    {
        try
        {
            foreach (var message in updateReply.Messages)
            {
                await sender.Send(updateReply.Update, message);
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Sending message failed");
            return false;
        }
    }

    private static MessageEntity GetBotCommand(Message message) => message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

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

        var commandText = update.Text.Substring(botCommand.Offset, botCommand.Length);

        var command = commandFactory.Create(commandText);;
        if (command is null)
            return null;

        parameter = update.Text.Substring(botCommand.Offset + botCommand.Length).Trim();

        // SwineId is null -> command in private chat and no private swine is selected -> prompt user to select private swine via PiggeryCommand
        if (update.SwineId is null && command is not INoSwineCommand)
        {
            command = commandFactory.Create<PiggeryCommand>();
            parameter = string.Empty;
        }

        return command;
    }
}

