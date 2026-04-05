using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Start;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger<StartCommand> logger, IMessageFactory messageFactory, IStartLinkParser parser) : ParameterizedCommand<StartMessage>(logger, messageFactory)
{
    public override string Name => "/start";
    public override string Description => "Вывести это сообщение \U0001F928";

    protected override BotMessage ExecuteWithParameter(Update update, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return CreateMessage();

        var didParse = parser.TryParse(parameter, out var action);

        if (!didParse)
        {
            logger.LogWarning("Failed to parse {0} as start link action", parameter);
            return CreateMessage();
        }

        var setSwineMessage = MessageFactory.Create<SetPrivateSwineMessage>();
        action.Execute(setSwineMessage);
        return setSwineMessage;
    }
}
