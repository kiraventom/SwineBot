using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class InvalidAchievementChecker(ILogger<InvalidAchievementChecker> Logger, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, Array.Empty<AchievementLevel>())
{
    public override AchievementType Type => AchievementType.None;

    protected override bool DoesLevelApply(int value, int level) => false;

    protected override Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId) => null;
}

