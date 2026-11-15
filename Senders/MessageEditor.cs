using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using SwineBot.BotMessages;

namespace SwineBot.Senders;

public interface IMessageEditor
{
    Task EditReplyMarkup(long chatId, int messageId, InlineKeyboardMarkup markup);
    Task RemoveReplyMarkup(long chatId, int messageId);
    Task EditText(long chatId, int messageId, string text);
    Task EditMessage(long chatId, int messageId, IBotMessage message);
}

public class MessageEditor(ITelegramBotClient client) : IMessageEditor
{
    public async Task EditReplyMarkup(long chatId, int messageId, InlineKeyboardMarkup markup)
    {
        await client.EditMessageReplyMarkup(chatId, messageId, markup);
    }

    public async Task EditText(long chatId, int messageId, string text)
    {
        await client.EditMessageText(chatId, messageId, text, parseMode: ParseMode.MarkdownV2);
    }

    /// <summary>
    /// Text and keyboard in a single request: a separate <c>EditMessageReplyMarkup</c>
    /// after a text edit fails with "message is not modified", because the text edit
    /// already drops the keyboard.
    /// </summary>
    public async Task EditMessage(long chatId, int messageId, IBotMessage message)
    {
        try
        {
            await client.EditMessageText(chatId, messageId, message.Text, parseMode: ParseMode.MarkdownV2, replyMarkup: message.Keyboard);
        }
        catch (ApiRequestException e) when (e.Message.Contains("message is not modified"))
        {
            // Повторное нажатие по той же кнопке: сообщение уже ровно такое, каким его хотят видеть
        }
    }

    public async Task RemoveReplyMarkup(long chatId, int messageId)
    {
        var empty = new InlineKeyboardMarkup();
        await client.EditMessageReplyMarkup(chatId, messageId, empty);
    }
}
