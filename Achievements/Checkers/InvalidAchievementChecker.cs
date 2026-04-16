using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class InvalidAchievementChecker(ILogger<InvalidAchievementChecker> logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.None;

    protected override bool DoesLevelApply(int value, int level) => false;

    protected override Task<int?> GetValue(ViewModel viewModel, int swineId) => null;
}

