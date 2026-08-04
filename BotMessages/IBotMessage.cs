using Telegram.Bot.Types.ReplyMarkups;

namespace SwineBot.BotMessages;

public interface IBotMessage
{
    bool Notify { get; }
    string Text { get; }
    byte[] PhotoBytes { get; }
    InlineKeyboardMarkup Keyboard { get; }
}
