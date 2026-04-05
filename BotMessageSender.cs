using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public interface IBotMessageSender
{
    Task<Message> Send(Update update, BotMessage botMessage);
}

public class BotMessageSender(ILogger<BotMessageSender> logger, UserContext context, ITelegramBotClient client, IMessageFactory messageFactory, AchievementController achievController) : IBotMessageSender
{
    public async Task<Message> Send(Update update, BotMessage botMessage)
    {
        botMessage = await InitMessage(update, botMessage);
        if (botMessage is null)
            return null;

        await SendAchievementMessages(update, botMessage);

        var message = await SendMessage(update, botMessage);

        if (botMessage is IPinnableMessage { ShouldPin: true })
            await PinMessage(update, message);

        return message;
    }

    private async Task<BotMessage> InitMessage(Update update, BotMessage botMessage)
    {
        try
        {
            if (update.SwineId is null)
            {
                if (botMessage is not IStaticMessage and not PiggeryMessage)
                    botMessage = messageFactory.Create<PiggeryMessage>();
            }

            await botMessage.Init(update);
            return botMessage;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to initialize {message}", botMessage.GetType().Name);

            if (botMessage is InvalidMessage)
            {
                logger.LogCritical(e, "Failed to initialize {invalidMessageName}, shit got real", nameof(InvalidMessage));
                return null;
            }

            var invalidMessage = messageFactory.Create<InvalidMessage>();
            return await InitMessage(update, invalidMessage);
        }
    }

    private async Task<Message> SendMessage(Update update, BotMessage botMessage)
    {
        try
        {
            var chatId = update.IsPrivateChat 
                ? context.Users.First(u => u.UserId == update.UserId).TelegramId
                : context.Groups.First(g => g.GroupId == update.GroupId).TelegramId;

            var text = botMessage.Text.ToString();
            var message = await SendMessage(chatId, botMessage.PhotoFilePath, text);

            logger.LogInformation("Sent \"{text}\" to chat [{id}], messageId [{messageId}]", text, chatId, message.MessageId);
            return message;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Sending message failed");
            return null;
        }
    }

    private async Task PinMessage(Update update, Message message)
    {
        if (message is null)
            return;

        if (!update.IsPrivateChat)
            return;

        var chatId = context.Users.First(u => u.UserId == update.UserId).TelegramId;

        try
        {
            await client.UnpinAllChatMessages(chatId);
            await client.PinChatMessage(chatId, message.MessageId);

            logger.LogInformation("Pinned [{messageId}] for user [{user}]", message.MessageId, update.UserId);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Pinning message [{messageId}] for user [{user}] failed", message.MessageId, update.UserId);
        }
    }

    private async Task<Message> SendMessage(ChatId chatId, string photoFilePath, string text)
    {
        if (photoFilePath is null)
            return await client.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.MarkdownV2, linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });

        Message message;
        try
        {
            using (var stream = File.OpenRead(photoFilePath))
            {
                var photo = InputFile.FromStream(stream);
                message = await client.SendPhoto(chatId: chatId, photo: photo, caption: text, parseMode: ParseMode.MarkdownV2);
            }
        }
        finally
        {
            if (File.Exists(photoFilePath))
                File.Delete(photoFilePath);
        }

        return message;
    }

    private async Task SendAchievementMessages(Update update, BotMessage botMessage)
    {
        if (update.SwineId is null || botMessage is AchievementMessage)
            return;

        var achievMessages = achievController.GetAchievMessages(update.SwineId.Value, botMessage);
        await foreach (var achievMessage in achievMessages)
            await Send(update, achievMessage);
    }
}
