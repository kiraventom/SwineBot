using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class PiggeryCommand(ILogger<PiggeryCommand> logger, IMessageFactory messageFactory) : Command<PiggeryMessage>(logger, messageFactory)
{
    public override string Name => "/piggery";
    public override string Description => "Осмотреть свинарник \U0001F6D6";
}

