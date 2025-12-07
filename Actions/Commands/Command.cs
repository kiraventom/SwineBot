using Serilog;

namespace SwineBot.Actions.Commands;

public abstract class Command(ILogger logger, BotMessageSender sender) : UserAction(logger, sender)
{
    public override bool IsMatch(string name)
    {
        var index = name.IndexOf('@');
        if (index != -1)
            return base.IsMatch(name.Substring(0, index));

        return base.IsMatch(name);
    }
}
