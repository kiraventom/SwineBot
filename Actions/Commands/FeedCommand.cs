using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Actions.Commands;

public class FeedCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/feed";

    public override Task ExecuteAsync(UserContext userContext, User user, string actionText)
    {
        var feedMessage = new FeedMessage(Logger);
        return Sender.Send(userContext, user.UserId, feedMessage);
    }
}

