using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/info";

    public override BotMessage Execute(string actionText)
    {
        return new InfoMessage(Logger);
    }
}
