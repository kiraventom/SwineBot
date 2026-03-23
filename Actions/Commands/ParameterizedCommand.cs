using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public abstract class ParameterizedCommand<T>(ILogger<ParameterizedCommand<T>> logger, IMessageFactory messageFactory) : Command<T>(logger, messageFactory) where T : BotMessage
{
    public override BotMessage Execute(string actionText)
    {
        var spaceIndex = actionText.IndexOf(' ');
        var parameter = spaceIndex == -1 ? string.Empty : actionText.Substring(spaceIndex);
        return CreateMessage(parameter);
    }
}

