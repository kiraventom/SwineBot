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
            AchievementType.Weight => new WeightChecker(loggerFactory.CreateLogger<WeightChecker>(), context, dtnProvider),
            AchievementType.WeightGain => new WeightGainChecker(loggerFactory.CreateLogger<WeightGainChecker>(), context, dtnProvider),
            AchievementType.WeightLoss => new WeightLossChecker(loggerFactory.CreateLogger<WeightLossChecker>(), context, dtnProvider),
            AchievementType.Overfeed => new OverfeedChecker(loggerFactory.CreateLogger<OverfeedChecker>(), context, dtnProvider),
            AchievementType.NoOverfeed => new NoOverfeedChecker(loggerFactory.CreateLogger<NoOverfeedChecker>(), context, dtnProvider),
            AchievementType.LowLuck => new LowLuckChecker(loggerFactory.CreateLogger<LowLuckChecker>(), context, dtnProvider),
            AchievementType.HighLuck => new HighLuckChecker(loggerFactory.CreateLogger<HighLuckChecker>(), context, dtnProvider),
            _ => new InvalidChecker(loggerFactory.CreateLogger<InvalidChecker>(), context, dtnProvider),
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

