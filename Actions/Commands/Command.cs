using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public abstract class Command<T>(ILogger<Command<T>> logger, IMessageFactory messageFactory) : UserAction(logger), ICommand where T : BotMessage
{
    public virtual string Title => Name;
    public abstract string Description { get; }

    public override bool IsMatch(string name)
    {
        var index = name.IndexOf('@');
        if (index != -1)
            return base.IsMatch(name.Substring(0, index));

        return base.IsMatch(name);
    }

    public override BotMessage Execute(string actionText) => CreateMessage();

    protected T CreateMessage(params object[] args) => messageFactory.Create<T>(args);
}
