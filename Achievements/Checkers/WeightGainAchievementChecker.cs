using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class WeightGainAchievementChecker(ILogger<WeightGainAchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> levels, IDateTimeNowProvider dtnProvider, UserContext context) : AchievementChecker(Logger, dtnProvider, context, levels)
{
    public override AchievementType Type => AchievementType.WeightGain;

    protected override Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel feedViewModel)
            return Task.FromResult<int?>(null);

        return Task.FromResult<int?>(feedViewModel.Result.Amount);
    }

    protected override bool DoesLevelApply(int value, int level) => value == level;
}
