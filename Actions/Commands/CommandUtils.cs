namespace SwineBot.Actions.Commands;

public static class CommandUtils
{
    public static bool IsMatch(string name, string text, string botUsername)
    {
        var index = text.IndexOf('@');
        if (index != -1)
        {
            var doesCommandMatch = name == text.Substring(0, index);
            var doesBotNameMatch = botUsername == text.Substring(index);
            return doesCommandMatch && doesBotNameMatch;
        }

        return name == text;
    }
}

