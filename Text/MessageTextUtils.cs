using System.Text;
using SwineBot.Achievements;

namespace SwineBot.Text;

public static class MessageTextUtils
{
    private static HashSet<char> CharsToEscape { get; } = [ '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!', '\\' ];

    public static StringBuilder EscapeString(string str, StringBuilder stringBuilder = null)
    {
        var sb = stringBuilder ?? new StringBuilder();
        foreach (var ch in str)
        {
            if (CharsToEscape.Contains(ch))
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

    public static string GetLastDTStr(IReadOnlyCollection<DateTime> recentDTs, DateTime current)
    {
        if (recentDTs.Count == 0)
            return "так давно, что никогда...";

        var lastDT = recentDTs.Max();
        var diff = current - lastDT;
        if (diff.TotalMinutes < 1)
        {
            return "Только что";
        }
        else if (diff.TotalHours < 1)
        {
            var totalMin = (int)diff.TotalMinutes;
            var minutesDecl = MessageTextUtils.GetDeclinatedNoun(totalMin, Unit.Minute);
            return $"{totalMin} {minutesDecl} назад";
        }
        else
        {
            var totalHours = (int)diff.TotalHours;
            var hoursDecl = MessageTextUtils.GetDeclinatedNoun(totalHours, Unit.Hour);
            return $"{totalHours} {hoursDecl} назад";
        }
    }
}

