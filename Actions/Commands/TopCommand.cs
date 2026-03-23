using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class TopCommand(ILogger<TopCommand> logger, IMessageFactory messageFactory) : Command<TopMessage>(logger, messageFactory)
{
    public override string Name => "/top";
    public override string Description => "Топ свинов \U0001f4cb";
}
