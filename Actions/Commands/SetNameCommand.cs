using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public class SetNameCommand(ILogger logger) : Command(logger)
{
    public const string COMMAND_NAME = "/setname";
    public override string Name => COMMAND_NAME;

    public override BotMessage Execute(string actionText)
    {
        var spaceIndex = actionText.IndexOf(' ');
        var name = spaceIndex == -1 ? null : actionText.Substring(spaceIndex);
        var newNameMessage = new NewNameMessage(Logger, name);
        return newNameMessage;
    }
}
