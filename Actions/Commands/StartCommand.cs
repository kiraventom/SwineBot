using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class StartCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public override string Name => "/start";

    public override Task ExecuteAsync(UserContext userContext, ChatId chatId, Model.User user, string actionText)
    {
        var startMessage = new StartMessage(Logger);
        return Sender.Send(userContext, chatId, user.UserId, startMessage);
    }
}
