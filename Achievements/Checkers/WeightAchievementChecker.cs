using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class WeightAchievementChecker(ILogger<WeightAchievementChecker> Logger, UserContext context, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger, dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.Weight;

    protected override Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId)
    {
        if (botMessage is not FeedMessage feedMessage)
            return null;

        var weight = context.Swines.First(s => s.SwineId == swineId).Weight;
        return Task.FromResult<int?>(weight);
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}
