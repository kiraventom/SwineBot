using Serilog;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class TopCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/top";

    public override BotMessage Execute(string actionText)
    {
        return new TopMessage(Logger);
    }
}
