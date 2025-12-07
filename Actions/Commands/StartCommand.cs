using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/start";

    public override Task ExecuteAsync(UserContext userContext, User user, string actionText)
    {
        var startMessage = new StartMessage(Logger);
        return Sender.Send(userContext, user.UserId, startMessage);
    }
}
