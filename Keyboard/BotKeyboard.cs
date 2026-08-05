using Telegram.Bot.Types.ReplyMarkups;

namespace SwineBot.Keyboard;

public class BotKeyboard
{
    private readonly List<List<InlineKeyboardButton>> _rows;

    public BotKeyboard()
    {
        _rows = new List<List<InlineKeyboardButton>>();
        NewRow();
    }

    public void AddSwitchInlineQueryCurrentChat(string text, string query)
    {
        _rows.Last().Add(new InlineKeyboardButton(text) { SwitchInlineQueryCurrentChat = query });
    }

    public void AddButton(string caption, string callbackData)
    {
        _rows.Last().Add(new InlineKeyboardButton(caption, callbackData));
    }

    public void NewRow() => _rows.Add(new List<InlineKeyboardButton>());

    public InlineKeyboardMarkup ToMarkup()
    {
        var markup = new InlineKeyboardMarkup();
        foreach (var row in _rows)
        {
            if (row.Count != 0)
                markup.AddNewRow(row.ToArray());
        }

        return markup;
    }
}
