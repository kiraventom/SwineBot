using System.Text;
using SwineBot.Achievements;

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

    public static string GetDeclinatedNoun(int count, Unit unit) => GetDeclinatedNoun(count, unit.Singular, unit.AccusativeSingular, unit.AccusativePlural);

    private static string GetDeclinatedNoun(int count, string singular, string accusativeSingular, string accusativePlural)
    {
        var lastTwoDigits = count % 100;

        if (lastTwoDigits is >= 10 and <= 20)
            return accusativePlural;

        var lastDigit = count % 10;
        return lastDigit switch
        {
            1 => singular,
            2 or 3 or 4 => accusativeSingular,
            _ => accusativePlural,
        };
    }
}

