using Serilog;
using SwineBot.Actions;
using SwineBot.Model;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public class TelegramController(ILogger logger, IReadOnlyCollection<UserAction> actions)
{
    private bool _started;

    public void StartReceiving(ITelegramBotClient client)
    {
        if (_started)
        {
            logger.Error("Tried to start {className} twice", nameof(TelegramController));
            return;
        }

        _started = true;

        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };

        client.StartReceiving(OnUpdate, OnError, receiverOptions);
        logger.Information("Started listening");
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        logger.Information("Received update: {updateType}", update.Type);

        using (var userContext = UserContext.Create())
        {
            using (var transaction = userContext.Database.BeginTransaction())
            {
                try
                {
                    if (update.Message is { } message )
                    {
                        await HandleMessageAsync(message, userContext);

                        userContext.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error(e.ToString());
                }
            }
        }
    }

    private async Task<bool> HandleMessageAsync(Message message, UserContext userContext)
    {
        var sender = message.From!;

        var user = userContext.GetOrAddUser(sender.Id, sender.FirstName, sender.Username);

        logger.Information("Received message [{messageId}] with text '{text}' in chat [{chatId}] from user [{userId}] '{firstname}'", message.MessageId, message.Text, message.Chat.Id, user.UserId, user.FirstName);

        var botCommand = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (botCommand is not null)
            return await HandleBotCommandAsync(userContext, message.Chat.Id, user, botCommand, message.Text);

        return false;
    }

    private async Task<bool> HandleBotCommandAsync(UserContext userContext, ChatId chatId, Model.User user, MessageEntity botCommand, string messageText)
    {
        var commandText = messageText.Substring(botCommand.Offset, botCommand.Length);
        return await HandleUserActionAsync(userContext, chatId, user, commandText, messageText);
    }

    private async Task<bool> HandleUserActionAsync(UserContext userContext, ChatId chatId, Model.User user, string actionText, string fullText)
    {
        var action = actions.FirstOrDefault(c => c.IsMatch(actionText));
        if (action is null)
        {
            logger.Warning("Action '{actionText}' does not match any of actions: [ {actions} ]", actionText, string.Join(", ", actions.Select(c => c.Name)));
            return false;
        }

        await action.ExecuteAsync(userContext, chatId, user, fullText);
        return true;
    }

    private static Task OnError(ITelegramBotClient client, Exception exception, CancellationToken ct) => Task.CompletedTask;
}
