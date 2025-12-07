using System.Text;

namespace SwineBot.Text;

public static class MessageTextUtils
{
    public static StringBuilder EscapeString(string str, StringBuilder stringBuilder = null)
    {
        var sb = stringBuilder ?? new StringBuilder();
        foreach (var ch in str)
        {
            var charCode = (int)ch;
            if (charCode is >= 1 and <= 126)
                sb.Append('\\');

            sb.Append(ch);
        }

        return sb;
    }

    public static string GetDeclinatedNoun(int days, string singular, string accusativeSingular, string accusativePlural)
    {
        var lastTwoDigits = days % 100;

        if (lastTwoDigits is >= 10 and <= 20)
            return accusativePlural;

        var lastDigit = days % 10;
        return lastDigit switch
        {
            1 => singular,
            2 or 3 or 4 => accusativeSingular,
            _ => accusativePlural,
        };
    }
}

