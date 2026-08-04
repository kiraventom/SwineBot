using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Updates;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Update = SwineBot.Updates.Update;

namespace SwineBot.Senders;

public interface IBotMessageSender
{
    Task<Message> Send(Update update, IBotMessage botMessage);
}

public class BotMessageSender(ILogger<BotMessageSender> logger, ITelegramBotClient client) : IBotMessageSender
{
    public async Task<Message> Send(Update update, IBotMessage botMessage)
    {
        var recepient = update.Recepient;

        if (botMessage is ICustomRecepientMessage { CustomRecepient: {} customRecepient })
            recepient = customRecepient;

        var message = await SendMessage(recepient, botMessage);

        if (botMessage is IPinnableMessage { ShouldPin: true })
            await PinMessage(recepient, message);

        return message;
    }

    private async Task<Message> SendMessage(Recepient recepient, IBotMessage botMessage)
    {
        var text = botMessage.Text;
        var replyMarkup = botMessage.Keyboard;
        var message = await SendMessage(recepient.ChatId, botMessage.PhotoBytes, text, replyMarkup);

        logger.LogInformation("Sent \"{text}\" to chat [{id}], messageId [{messageId}]", text, recepient.ChatId, message.MessageId);
        return message;
    }

    private async Task PinMessage(Recepient recepient, Message message)
    {
        if (message is null)
            return;

        if (!recepient.IsPrivateChat)
            return;

        await client.UnpinAllChatMessages(recepient.ChatId);
        await client.PinChatMessage(recepient.ChatId, message.MessageId);

        logger.LogInformation("Pinned [{messageId}] for chat [{chat}]", message.MessageId, recepient.ChatId);
    }

    private async Task<Message> SendMessage(ChatId chatId, byte[] photoBytes, string text, InlineKeyboardMarkup replyMarkup = null)
    {
        if (photoBytes is null)
            return await client.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.MarkdownV2, linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true }, replyMarkup: replyMarkup);

        Message message;
        using (var stream = new MemoryStream(photoBytes))
        {
            var photo = InputFile.FromStream(stream);
            message = await client.SendPhoto(chatId: chatId, photo: photo, caption: text, parseMode: ParseMode.MarkdownV2, replyMarkup: replyMarkup);
        }

        return message;
    }
}
