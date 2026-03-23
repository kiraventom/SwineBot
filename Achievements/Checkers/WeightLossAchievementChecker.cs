using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightLossAchievementChecker(ILogger<WeightLossAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.WeightLoss;

    protected override int? GetValue(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        return feedMessage.FeedResult.Amount;
    }

    protected override bool DoesLevelApply(int value, int level)
    {
        return value <= level;
    }

    protected override bool IsSilentApply(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
        {
            Logger.LogError("{botMessage} is not {FeedMessage}", nameof(botMessage), nameof(FeedMessage));
            return true;
        }

        return swine.Weight != feedMessage.FeedResult.NewWeight;
    }
}
