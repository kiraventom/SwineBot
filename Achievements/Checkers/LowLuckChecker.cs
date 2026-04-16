using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class LowLuckChecker(ILogger<LowLuckChecker> logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.LowLuck;

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
    {
        if (viewModel is not FeedViewModel)
            return null;

        var feeds = Context.Feeds
            .Where(f => f.SwineId == swineId)
            .OrderByDescending(f => f.DateTime)
            .AsAsyncEnumerable();

        int lowLuckCount = 0;
        await foreach (var feed in feeds)
        {
            if (feed.Luck > FeedCommand.LOW_LUCK_THRESHOLD)
                break;

            ++lowLuckCount;
        }

        return lowLuckCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}

