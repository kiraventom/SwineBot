using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class WeightChecker(ILogger<WeightChecker> Logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(Logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.Weight;

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel)
            return null;

        var weight = (await Context.Swines.FirstAsync(s => s.SwineId == swineId)).Weight;
        return weight;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}
