using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class WeightLossAchievementChecker(ILogger<WeightLossAchievementChecker> Logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.WeightLoss;

    protected override Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel feedViewModel)
            return Task.FromResult<int?>(null);

        return Task.FromResult<int?>(feedViewModel.Result.Amount);
    }

    protected override bool DoesLevelApply(int value, int level)
    {
        return value <= level;
    }
}
