using Serilog;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class HistoryAllCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/historyall";

    public override BotMessage Execute(string actionText)
    {
        return new HistoryAllMessage(Logger);
    }
}

