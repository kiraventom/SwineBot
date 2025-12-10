using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class SetNameCommand(ILogger logger, BotMessageSender sender) : Command(logger, sender)
{
    public const string COMMAND_NAME = "/setname";
    public override string Name => COMMAND_NAME;

    public override Task ExecuteAsync(UserContext userContext, ChatId chatId, Model.User user, string actionText)
    {
        var name = actionText.Substring(this.Name.Length);
        var newNameMessage = new NewNameMessage(Logger, name);
        return Sender.Send(userContext, chatId, user.UserId, newNameMessage);
    }
}
