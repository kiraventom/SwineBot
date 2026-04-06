using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public interface IBotMessageSender
{
    Task<Message> Send(Update update, IBotMessage botMessage);
}

public class BotMessageSender(ILogger<BotMessageSender> logger, ITelegramBotClient client) : IBotMessageSender
{
    public async Task<Message> Send(Update update, IBotMessage botMessage)
    {
        var message = await SendMessage(update, botMessage);

        if (botMessage is IPinnableMessage { ShouldPin: true })
            await PinMessage(update, message);

        return message;
    }

    private async Task<Message> SendMessage(Update update, IBotMessage botMessage)
    {
        try
        {
            var text = botMessage.Text;
            var message = await SendMessage(update.TelegramChatId, botMessage.PhotoBytes, text);

            logger.LogInformation("Sent \"{text}\" to chat [{id}], messageId [{messageId}]", text, update.TelegramChatId, message.MessageId);
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

        try
        {
            await client.UnpinAllChatMessages(update.TelegramChatId);
            await client.PinChatMessage(update.TelegramChatId, message.MessageId);

            logger.LogInformation("Pinned [{messageId}] for user [{user}]", message.MessageId, update.UserId);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Pinning message [{messageId}] for user [{user}] failed", message.MessageId, update.UserId);
        }
    }

    private async Task<Message> SendMessage(ChatId chatId, byte[] photoBytes, string text)
    {
        if (photoBytes is null)
            return await client.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.MarkdownV2, linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });

        Message message;
        using (var stream = new MemoryStream(photoBytes))
        {
            var photo = InputFile.FromStream(stream);
            message = await client.SendPhoto(chatId: chatId, photo: photo, caption: text, parseMode: ParseMode.MarkdownV2);
        }

        return message;
    }
}
