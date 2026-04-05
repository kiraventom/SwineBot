using System.Text;
using SwineBot.Model;

namespace SwineBot.Text;

// TODO Refactor to List<TextPart>, TextPart.IsBold/IsItalic/IsUnderline etc., merge them and turn into text on ToString()
public class MessageText
{
    private readonly StringBuilder _sb = new();
    private const string TAB = "   ";
    private int _tabLevel = 0;

    public const string DOT_SPACE = "⋅ ";

    public MessageText(string text = null)
    {
        if (text is not null)
        {
            Verbatim(text);
        }
    }

    public MessageText LineBreak()
    {
        _sb.AppendLine();
        return this;
    }

    public MessageText Tab(Action<MessageText> action)
    {
        ++_tabLevel;
        action.Invoke(this);
        --_tabLevel;

        return this;
    }

    public MessageText Verbatim(object o)
    {
        var text = o?.ToString();

        if (text is null)
            return this;

        ApplyTabLevel(_tabLevel, _sb);
        MessageTextUtils.EscapeString(text, _sb);

        return this;
    }

    public MessageText Bold(object o)
    {
        var text = o?.ToString();

        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 0 && _sb[_sb.Length - 1] == '*')
            _sb.Remove(_sb.Length - 1, 1);
        else
            _sb.Append('*');

        Verbatim(text);
        _sb.Append('*');

        return this;
    }

    public MessageText Italic(object obj)
    {
        ApplyTabLevel(_tabLevel, _sb);

        var text = obj.ToString();

        if ((_sb.Length == 1 && _sb[_sb.Length - 1] == '_') || (_sb.Length > 1 && _sb[_sb.Length - 1] == '_' && _sb[_sb.Length - 2] != '_'))
            _sb.Remove(_sb.Length - 1, 1);
        else
            _sb.Append('_');

        Verbatim(text);
        _sb.Append('_');

        return this;
    }

    public MessageText ItalicBold(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 1 && _sb[_sb.Length - 2] == '_' && _sb[_sb.Length - 1] == '*')
            _sb.Remove(_sb.Length - 2, 2);
        else
            _sb.Append('*').Append('_');

        Verbatim(text);
        _sb.Append('_').Append('*');

        return this;
    }

    public MessageText Underline(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 1 && _sb[_sb.Length - 2] == '_' && _sb[_sb.Length - 1] == '_')
            _sb.Remove(_sb.Length - 2, 2);
        else
            _sb.Append('_').Append('_');

        Verbatim(text);
        _sb.Append('_').Append('_');

        return this;
    }

    public MessageText Strikethrough(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 0 && _sb[_sb.Length - 1] == '~')
            _sb.Remove(_sb.Length - 1, 1);
        else
            _sb.Append('~');

        Verbatim(text);
        _sb.Append('~');

        return this;
    }

    public MessageText Spoiler(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 0 && _sb[_sb.Length - 2] == '|' && _sb[_sb.Length - 1] == '|')
            _sb.Remove(_sb.Length - 2, 2);
        else
            _sb.Append("||");

        Verbatim(text);
        _sb.Append("||");

        return this;
    }

    public MessageText InlineUrl(string text, string link)
    {
        ApplyTabLevel(_tabLevel, _sb);

        _sb.Append('[');
        Verbatim(text);
        _sb.Append(']');

        var escapedLink = EscapeLink(link);
        _sb.Append('(').Append(escapedLink).Append(')');
        return this;
    }

    public MessageText InlineMention(User user, string text = null)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (text is null)
            text = user.FirstName;

        return string.IsNullOrEmpty(user.Tag)
            ? InlineMention(text, user.TelegramId)
            : InlineMention(text, user.Tag);
    }

    public MessageText InlineMention(string text, string tag)
    {
        ApplyTabLevel(_tabLevel, _sb);
        
        _sb.Append('[');
        Verbatim(text);
        _sb.Append(']');

        _sb.Append('(')
            .Append(@"t.me/")
            .Append(tag)
            .Append(')');

        return this;
    }

    public MessageText InlineMention(string text, long userId)
    {
        ApplyTabLevel(_tabLevel, _sb);

        _sb.Append('[');
        Verbatim(text);
        _sb.Append(']');

        _sb.Append('(')
            .Append(@"tg://user?id=")
            .Append(userId)
            .Append(')');

        return this;
    }

    public MessageText Monospace(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        if (_sb.Length > 0 && _sb[_sb.Length - 1] == '`')
            _sb.Remove(_sb.Length - 1, 1);
        else
            _sb.Append('`');

        Verbatim(text);
        _sb.Append('`');

        return this;
    }

    public MessageText Quote(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        // Empty bold definition to separate from previous quote
        _sb.Append("**");
        var lines = text.Split('\n');

        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            _sb.Append('>');
            Verbatim(line);

            if (i < lines.Length - 1)
                _sb.AppendLine();
        }

        return this;
    }

    public MessageText ExpandableQuote(string text)
    {
        ApplyTabLevel(_tabLevel, _sb);

        Quote(text);

        _sb.Append("||");
        return this;
    }

    private static void ApplyTabLevel(int level, StringBuilder sb)
    {
        if (sb.Length == 0 || sb[sb.Length - 1] == '\n')
        {
            for (int i = 0; i < level; ++i)
                sb.Append(TAB);
        }
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private static string EscapeLink(string link) => link.Replace("\\", @"\\").Replace(")", "\\)");
}
