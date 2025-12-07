using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class FeedCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/feed";

    public override Task ExecuteAsync(UserContext userContext, ChatId chatId, Model.User user, string actionText)
    {
        var feedMessage = new FeedMessage(Logger);
        return Sender.Send(userContext, chatId, user.UserId, feedMessage);
    }
}

