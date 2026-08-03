using Telegram.Bot.Types.ReplyMarkups;

namespace SwineBot.Keyboard;

public class BotKeyboard
{
    public InlineKeyboardMarkup Markup { get; private set; } = null;

    public void SetSwitchInlineQueryCurrentChat(string text, string query)
    {
        Markup = new(new InlineKeyboardButton(text) { SwitchInlineQueryCurrentChat = query });
    }
}
