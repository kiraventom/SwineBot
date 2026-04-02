using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;

namespace SwineBot.Actions.Commands;

public class FeedCommand(ILogger<FeedCommand> logger, IMessageFactory messageFactory) : Command<FeedMessage>(logger, messageFactory)
{
    public override string Name => "/feed";
    public override string Description => "Покормить своего свина \U0001F416";
}

