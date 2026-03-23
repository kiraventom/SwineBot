using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class HistoryCommand(ILogger<HistoryCommand> logger, IMessageFactory messageFactory) : Command<HistoryMessage>(logger, messageFactory)
{
    public override string Name => "/history";
    public override string Description => "История веса свинок \U0001f4c8";
}
