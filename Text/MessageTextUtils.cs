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
}

