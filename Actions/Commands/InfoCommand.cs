using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/info";

    public override Task ExecuteAsync(UserContext userContext, User user, string actionText)
    {
        var startMessage = new InfoMessage(Logger);
        return Sender.Send(userContext, user.UserId, startMessage);
    }
}

