using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightAchievementChecker(ILogger<WeightAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.Weight;

    protected override int? GetValue(BotMessage botMessage, Swine swine)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        return swine.Weight;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;

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
