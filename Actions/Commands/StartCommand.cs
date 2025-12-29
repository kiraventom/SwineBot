using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/start";

    public override BotMessage Execute(string actionText)
    {
        return new StartMessage(Logger);
    }
}
