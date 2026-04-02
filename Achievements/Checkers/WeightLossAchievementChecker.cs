using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightLossAchievementChecker(ILogger<WeightLossAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger, dtnProvider, levels)
{
    public override AchievementType Type => AchievementType.WeightLoss;

    protected override int? GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        return feedMessage.FeedResult.Amount;
    }

    protected override bool DoesLevelApply(int value, int level)
    {
        return value <= level;
    }
}
