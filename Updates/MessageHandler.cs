using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SwineBot.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SwineBot.Actions.Commands;
using SwineBot.Senders;
using SwineBot.Actions.Commands.Duel;

namespace SwineBot.Updates;

public class MessageHandler(ILogger<MessageHandler> logger, UserContext context, UserContextHelpers contextHelpers, Config config, IBotMessageSender sender, ICommandFactory commandFactory) : UpdateTypeHandler<Telegram.Bot.Types.Message>
{
    public override async Task<UpdateHandleResult> Handle(Message message, CancellationToken token)
    {
        if (message.ForwardOrigin is not null)
            return UpdateHandleResult.MessageForward;

        // Group transformed into supergroup
        if (message.MigrateFromChatId is {} migrateFromChatId)
        {
            var newChatId = message.Chat.Id;
            var group = await context.Groups.AsTracking().FirstOrDefaultAsync(g => g.TelegramId == migrateFromChatId);
            if (group is not null)
            {
                group.TelegramId = newChatId;
                await context.SaveChangesAsync();

                logger.LogInformation("Migrated group {groupId} from {oldChatId} to {newChatId}", group.GroupId, migrateFromChatId, newChatId);
                return UpdateHandleResult.MessageSuccesfulMigration;
            }
        }

        var botCommand = GetBotCommand(message);
        if (botCommand is null)
        {
            if (message.Text is { } text)
                logger.LogInformation("Not command: {text}", text);

            return UpdateHandleResult.MessageNotCommand;
        }

        return await HandleCommand(message, botCommand, token);
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

    private async Task<UpdateHandleResult> HandleCommand(Message tgMessage, MessageEntity botCommand, CancellationToken token)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        try
        {
            var update = await TryRunAsync(
                    () => CreateUpdate(tgMessage),
                    "Failed to create update", UpdateHandleResult.MessageFailedToCreateUpdate);

            var (command, parameter) = TryRun(
                    () =>
                    {
                        var cmd = GetCommand(update, botCommand, out var p);
                        return (cmd, p);
                    },
                    "Failed to get command", UpdateHandleResult.MessageUnknownCommand);

            var messages = await TryRunAsync(
                    () => command.Execute(update, parameter),
                    "Failed to execute command", UpdateHandleResult.CommandFailed);

            await TryRunAsync(async () =>
                    {
                        foreach (var message in messages)
                        {
                            var sentMessage = await sender.Send(update, message);
                            await command.AfterMessageSend(update, message, sentMessage);
                        }
                    }, "Sending message failed", UpdateHandleResult.SendMessageFail);

            await TryRunAsync(async () =>
                    {
                        await context.SaveChangesAsync(token);
                        await transaction.CommitAsync(token);
                    }, "Failed to save database", UpdateHandleResult.DatabaseFail);

            return UpdateHandleResult.MessageOK;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(token);

            return e is UpdateHandleException updateEx
                ? updateEx.Result
                : UpdateHandleResult.UnknownError;
        }
    }

    private static MessageEntity GetBotCommand(Message message) => message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

    private ICommand GetCommand(Update update, MessageEntity botCommand, out string parameter)
    {
        parameter = null;

        var commandText = update.Text.Substring(botCommand.Offset, botCommand.Length);

        var command = commandFactory.Create(commandText);

        parameter = update.Text.Substring(botCommand.Offset + botCommand.Length).Trim();

        // SwineId is null -> command in private chat and no private swine is selected -> prompt user to select private swine via PiggeryCommand
        if (update.SwineId is null && command is not INoSwineCommand)
        {
            logger.LogInformation("Received private command that requires selected swine, replacing command with {piggeryCommand}", nameof(PiggeryCommand));
            command = commandFactory.Create<PiggeryCommand>();
            parameter = string.Empty;
        }
        else if (context.DuelRequests.FirstOrDefault(dr => dr.DefenderId == update.SwineId) is {} duelRequest && command is IActionCommand)
        {
            logger.LogInformation("Received command that performs action when duel request is not answered, replacing command with {duelReminderCommand}", nameof(DuelReminderCommand));
            command = commandFactory.Create<DuelReminderCommand>();
            parameter = string.Empty;
        }

        return command;
    }

    private async Task<T> TryRunAsync<T>(Func<Task<T>> action, string log, UpdateHandleResult err)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, log);
            throw new UpdateHandleException(err, ex);
        }
    }

    private async Task TryRunAsync(Func<Task> action, string log, UpdateHandleResult err)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, log);
            throw new UpdateHandleException(err, ex);
        }
    }

    private T TryRun<T>(Func<T> action, string log, UpdateHandleResult err)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, log);
            throw new UpdateHandleException(err, ex);
        }
    }
}

