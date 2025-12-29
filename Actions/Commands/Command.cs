using Serilog;

namespace SwineBot.Actions.Commands;

public abstract class Command(ILogger logger) : UserAction(logger)
{
    public override bool IsMatch(string name)
    {
        var index = name.IndexOf('@');
        if (index != -1)
            return base.IsMatch(name.Substring(0, index));

        return base.IsMatch(name);
    }
}
