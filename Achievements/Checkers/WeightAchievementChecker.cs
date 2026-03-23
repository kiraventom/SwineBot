using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightAchievementChecker(ILogger<WeightAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(Logger, levels)
{
    public override AchievementType Type => AchievementType.Weight;

    protected override int? GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var weight = context.Swines.First(s => s.SwineId == swineId).Weight;
        return weight;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}
