using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class FeedCommand(ILogger logger) : Command(logger)
{
    public override string Name => "/feed";

    public override BotMessage Execute(string actionText)
    {
        return new FeedMessage(Logger);
    }
}

