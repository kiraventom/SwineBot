using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.Actions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public interface IUpdateHandler
{
    Task Handle(Update update, CancellationToken token);
}

public class UpdateHandler(ILogger<UpdateHandler> Logger, UserContext Context, Config Config, IBotMessageSender Sender, IEnumerable<UserAction> actions) : IUpdateHandler
{
    private IReadOnlyCollection<UserAction> Actions { get; } = actions.ToList();

    public async Task Handle(Update update, CancellationToken token)
    {
        Logger.LogInformation("Received update: {updateType}", update.Type);

        using var transaction = Context.Database.BeginTransaction();
        try
        {
            if (update.Message is not { } message)
                return;

            var didHandle = await HandleMessageAsync(message);

            if (didHandle)
                Context.SaveChanges();

            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Logger.LogError(e, "Transaction failed, rolling back");
        }
    }

    private async Task<bool> HandleMessageAsync(Message message)
    {
        var sender = message.From;
        var chat = message.Chat;

        if (sender is null)
        {
            Logger.LogWarning("Sender is null");
            return false;
        }

        // Bot received its own message (e.g., pinned message notification)
        if (sender.Username == Config.Username)
            return false;

        var isPrivate = chat.Id == sender.Id;

        var user = Context.GetOrAddUser(chat.Id, chat.Title, sender.Id, sender.FirstName, sender.Username);

        if (isPrivate)
        {
            Logger.LogInformation("Received private message [{messageId}] with text '{text}' from user [{userId}] '{firstname}'", message.MessageId, message.Text, user.UserId, user.FirstName);
        }
        else
        {
            Logger.LogInformation("Received message [{messageId}] with text '{text}' in chat [{chatId}] from user [{userId}] '{firstname}'", message.MessageId, message.Text, message.Chat.Id, user.UserId, user.FirstName);
        }

        var botCommand = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (botCommand is not null)
            return await HandleBotCommandAsync(message.Chat.Id, user, botCommand, message.Text);

        return false;
    }

    private async Task<bool> HandleBotCommandAsync(ChatId chatId, Model.User user, MessageEntity botCommand, string messageText)
    {
        var commandText = messageText.Substring(botCommand.Offset, botCommand.Length);
        return await HandleUserActionAsync(chatId, user, commandText, messageText);
    }

    private async Task<bool> HandleUserActionAsync(ChatId chatId, Model.User user, string actionText, string fullText)
    {
        var action = Actions.FirstOrDefault(c => c.IsMatch(actionText));
        if (action is null)
        {
            Logger.LogWarning("Action '{actionText}' does not match any of actions: [ {actions} ]", actionText, string.Join(", ", Actions.Select(c => c.Name)));
            return false;
        }

        var botMessage = action.Execute(user.UserId, fullText);
        await Sender.Send(Context, chatId, user.UserId, botMessage);
        return true;
    }
}


