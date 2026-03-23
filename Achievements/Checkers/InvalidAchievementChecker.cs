using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class InvalidAchievementChecker(ILogger<InvalidAchievementChecker> Logger) : AchievementChecker(Logger, Array.Empty<AchievementLevel>())
{
    public override AchievementType Type => AchievementType.None;

    protected override bool DoesLevelApply(int value, int level) => false;

    protected override int? GetValue(BotMessage botMessage, UserContext context, int swineId) => null;
}

