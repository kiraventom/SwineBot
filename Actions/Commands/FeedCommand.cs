using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/feed",  "Покормить своего свина \U0001F416")]
public class FeedCommand(IMessageFactory messageFactory, IFeedGeneratorFactory feedGeneratorFactory, UserContext context, AchievementController achievController) : Command<FeedMessage, FeedViewModel>(messageFactory, achievController)
{
    public const double LOW_LUCK_THRESHOLD = 0.15;
    public const double HIGH_LUCK_THRESHOLD = 0.85;

    protected override async Task<FeedViewModel> ExecuteInternal(Update update, string parameter)
    {
        var feedManager = await feedGeneratorFactory.Create(update.SwineId);
        var result = await feedManager.Generate();

        var feedViewModel = result.Result switch
        {
            Result.FirstFeed => await HandleFeed(result, update),
            Result.Overfeed => await HandleFeed(result, update),
            Result.Throwup => await HandleThrowup(result, update),
            Result.Full => await HandleFull(result, update),
            _ => throw new NotSupportedException($"Unexpected feed result type \"{result.Result.ToString()}\"")
        };

        return feedViewModel;
    }

    private async Task<FeedViewModel> HandleFull(FeedResult result, Update update)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        return new FeedViewModel(result, swine.Name, 0);
    }

    private async Task<FeedViewModel> HandleThrowup(FeedResult result, Update update)
    {
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);
        swine.Weight = result.NewWeight;

        context.WeightLosses.Add(new WeightLoss()
        {
            SwineId = swine.SwineId,
            DateTime = result.UtcDT,
            IsThrowUp = true,
            Amount = result.Amount,
            Luck = result.Luck
        });

        var consecutiveOverfeeds = await OverfeedChecker.CountConsecutiveOverfeeds(context, swine.SwineId);

        return new FeedViewModel(result, swine.Name, consecutiveOverfeeds);
    }

    private async Task<FeedViewModel> HandleFeed(FeedResult result, Update update)
    {
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);
        swine.Weight = result.NewWeight;

        context.Feeds.Add(new Model.Feed()
        {
            SwineId = swine.SwineId,
            DateTime = result.UtcDT,
            Amount = result.Amount,
            Luck = result.Luck
        });

        return new FeedViewModel(result, swine.Name, 0);
    }
}

