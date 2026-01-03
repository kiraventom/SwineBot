using Serilog;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class HistoryCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/history";

    public override BotMessage Execute(string actionText)
    {
        return new HistoryMessage(Logger);
    }
}


