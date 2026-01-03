using Serilog;
using SwineBot.BotMessages;

namespace SwineBot.Actions;

public abstract class UserAction(ILogger logger)
{
    protected ILogger Logger { get; } = logger;

    public abstract string Name { get; }

    public abstract BotMessage Execute(string actionText);

    public virtual bool IsMatch(string name) => name == Name;
}

