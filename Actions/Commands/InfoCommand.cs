using Serilog;
using SwineBot.Achievements;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger logger, AchievementController achievController) : Command(logger)
{
    public override string Name => "/info";

    public override BotMessage Execute(string actionText)
    {
        return new InfoMessage(Logger, achievController);
    }
}
