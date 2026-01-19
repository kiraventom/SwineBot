using Serilog;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class SlaughterCommand(ILogger logger) : Command(logger)
{
    public const string COMMAND_NAME = "/slaughter";
    public override string Name => COMMAND_NAME;

    public override BotMessage Execute(string actionText)
    {
        var spaceIndex = actionText.IndexOf(' ');
        var name = spaceIndex == -1 ? null : actionText.Substring(spaceIndex);
        var newNameMessage = new SlaughterMessage(Logger, name);
        return newNameMessage;
    }
}

