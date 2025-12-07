using Serilog;
using SwineBot.Model;

namespace SwineBot.Actions;

public abstract class UserAction(ILogger logger, BotMessageSender sender)
{
    protected ILogger Logger { get; } = logger;
    protected BotMessageSender Sender { get; } = sender;

    public abstract string Name { get; }

    public abstract Task ExecuteAsync(UserContext userContext, User user, string actionText);

    public virtual bool IsMatch(string name) => name == Name;
}

