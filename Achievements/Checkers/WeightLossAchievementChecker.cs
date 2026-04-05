using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightLossAchievementChecker(ILogger<WeightLossAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.WeightLoss;

    protected override Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        return Task.FromResult<int?>(feedMessage.FeedResult.Amount);
    }

    protected override bool DoesLevelApply(int value, int level)
    {
        return value <= level;
    }
}
