using Serilog;
using SwineBot.Achievements;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class AchievCommand(ILogger logger, AchievementController achievController) : Command(logger)
{
    public override string Name => "/achiev";

    public override BotMessage Execute(string actionText)
    {
        return new AchievementsMessage(Logger, achievController);
    }
}

