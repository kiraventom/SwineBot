using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Start;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger<StartCommand> logger, IMessageFactory messageFactory, IServiceScopeFactory spf) : ParameterizedCommand<StartMessage>(logger, messageFactory)
{
    public override string Name => "/start";
    public override string Description => "Вывести это сообщение \U0001F928";

    protected override BotMessage ExecuteWithParameter(int userId, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return CreateMessage();

        using var scope = spf.CreateScope();
        var parser = scope.ServiceProvider.GetRequiredService<IStartLinkParser>();

        var didParse = parser.TryParse(parameter, out var action);

        if (!didParse)
        {
            logger.LogWarning("Failed to parse {0} as start link action", parameter);
            return CreateMessage();
        }

        var setSwineMessage = messageFactory.Create<SetPrivateSwineMessage>(userId);
        action.Execute(setSwineMessage);
        return setSwineMessage;
    }
}
