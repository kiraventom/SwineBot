using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class InvalidAchievementChecker(ILogger<InvalidAchievementChecker> Logger, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, Array.Empty<AchievementLevel>())
{
    public override AchievementType Type => AchievementType.None;

    protected override bool DoesLevelApply(int value, int level) => false;

    protected override Task<int?> GetValue(ViewModel viewModel, UserContext context, int swineId) => null;
}

