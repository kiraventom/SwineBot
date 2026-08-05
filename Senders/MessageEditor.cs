using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace SwineBot.Senders;

public interface IMessageEditor
{
    Task EditReplyMarkup(long chatId, int messageId, InlineKeyboardMarkup markup);
    Task RemoveReplyMarkup(long chatId, int messageId);
    Task EditText(long chatId, int messageId, string text);
    Task EditMessage(long chatId, int messageId, IBotMessage message);
}

public class MessageEditor(ILogger<MessageEditor> logger, ITelegramBotClient client) : IMessageEditor
{
    public async Task EditReplyMarkup(long chatId, int messageId, InlineKeyboardMarkup markup)
    {
        await client.EditMessageReplyMarkup(chatId, messageId, markup);
    }

    public async Task EditText(long chatId, int messageId, string text)
    {
        await client.EditMessageText(chatId, messageId, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
    }

    public async Task EditMessage(long chatId, int messageId, IBotMessage message)
    {
        await EditText(chatId, messageId, message.Text);
        await EditReplyMarkup(chatId, messageId, message.Keyboard);
    }

    public async Task RemoveReplyMarkup(long chatId, int messageId)
    {
        var empty = new InlineKeyboardMarkup();
        await client.EditMessageReplyMarkup(chatId, messageId, empty);
    }
}

