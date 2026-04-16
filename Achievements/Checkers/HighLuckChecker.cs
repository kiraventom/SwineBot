using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public class HighLuckChecker(ILogger<HighLuckChecker> logger, UserContext context, IDateTimeNowProvider dtnProvider) : AchievementChecker(logger, dtnProvider, context)
{
    public override AchievementType Type => AchievementType.HighLuck;

    protected override async Task<int?> GetValue(ViewModel viewModel, int swineId)
{
        if (viewModel is not FeedViewModel)
            return null;

        var feeds = Context.Feeds
            .Where(f => f.SwineId == swineId)
            .OrderByDescending(f => f.DateTime)
            .AsAsyncEnumerable();

        int highLuckCount = 0;
        await foreach (var feed in feeds)
        {
            if (feed.Luck < FeedCommand.HIGH_LUCK_THRESHOLD)
                break;

            ++highLuckCount;
        }

        return highLuckCount;
    }

    protected override bool DoesLevelApply(int value, int level) => value >= level;
}

