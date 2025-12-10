using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/info";

    public override Task ExecuteAsync(UserContext userContext, ChatId chatId, Model.User user, string actionText)
    {
        var infoMessage = new InfoMessage(Logger);
        return Sender.Send(userContext, chatId, user.UserId, infoMessage);
    }
}
