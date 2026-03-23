using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger<StartCommand> logger, IMessageFactory messageFactory) : Command<StartMessage>(logger, messageFactory)
{
    public override string Name => "/start";
    public override string Description => "Вывести это сообщение \U0001F928";
}
