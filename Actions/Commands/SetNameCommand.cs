using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class SetNameCommand(ILogger<SetNameCommand> logger, IMessageFactory messageFactory) : ParameterizedCommand<NewNameMessage>(logger, messageFactory)
{
    public const string COMMAND_NAME = "/setname";
    public override string Name { get; } = COMMAND_NAME;
    public override string Title { get; } = COMMAND_NAME + " <имя>";
    public override string Description => "Поменять имя свинки \u270f";
}
