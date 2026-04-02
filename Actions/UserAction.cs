using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions;

public abstract class UserAction(ILogger<UserAction> logger)
{
    protected ILogger Logger { get; } = logger;

    public abstract string Name { get; }

    public abstract BotMessage Execute(int userId, string actionText);

    public virtual bool IsMatch(string name) => name == Name;
}


