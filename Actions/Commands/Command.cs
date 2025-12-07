using Serilog;

namespace SwineBot.Actions.Commands;

public abstract class Command(ILogger logger, BotMessageSender sender) : UserAction(logger, sender)
{
    public override bool IsMatch(string name)
    {
        var commandText = name.Substring(0, name.IndexOf('@'));
        return base.IsMatch(commandText);
    }
}
