using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class AchievementCheckerFactory(ILoggerFactory loggerFactory, UserContext context, IDateTimeNowProvider dtnProvider, IReadOnlyDictionary<AchievementType, AchievementData> achievementDatas)
{
    public AchievementChecker Build(AchievementType type)
    {
        var data = achievementDatas[type];

        AchievementChecker checker = type switch
        {
            AchievementType.Weight => new WeightAchievementChecker(loggerFactory.CreateLogger<WeightAchievementChecker>(), context, dtnProvider),
            AchievementType.WeightGain => new WeightGainAchievementChecker(loggerFactory.CreateLogger<WeightGainAchievementChecker>(), context, dtnProvider),
            AchievementType.WeightLoss => new WeightLossAchievementChecker(loggerFactory.CreateLogger<WeightLossAchievementChecker>(), context, dtnProvider),
            AchievementType.Overfeed => new OverfeedAchievementChecker(loggerFactory.CreateLogger<OverfeedAchievementChecker>(), context, dtnProvider),
            AchievementType.NoOverfeed => new NoOverfeedAchievementChecker(loggerFactory.CreateLogger<NoOverfeedAchievementChecker>(), context, dtnProvider),
            _ => new InvalidAchievementChecker(loggerFactory.CreateLogger<InvalidAchievementChecker>(), context, dtnProvider),
        };

        checker.Init(data.Levels, data.IsArchived);
        return checker;
    }

    public IEnumerable<AchievementChecker> BuildAll()
    {
        foreach (var data in achievementDatas)
            yield return Build(data.Key);
    }
}

