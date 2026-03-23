using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class SlaughterCommand(ILogger<SlaughterCommand> logger, IMessageFactory messageFactory) : ParameterizedCommand<SlaughterMessage>(logger, messageFactory)
{
    public const string COMMAND_NAME = "/slaughter";
    public override string Name => COMMAND_NAME;
    public override string Description => "Убить свинку \U0001f52a";
}

