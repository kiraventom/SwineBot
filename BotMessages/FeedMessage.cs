using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class FeedMessage(ILogger<FeedMessage> Logger, IFeedGeneratorFactory FeedGeneratorFactory) : BotMessage(Logger)
{
    private const double LOW_LUCK_THRESHOLD = 0.15;
    private const double HIGH_LUCK_THRESHOLD = 0.85;

    public FeedResult FeedResult { get; private set; }

    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var feedManager = FeedGeneratorFactory.Create(swineId);
        var result = feedManager.Generate();

        switch (result.Result)
        {
            case Result.FirstFeed:
            case Result.Overfeed:
                HandleFeed(feedManager.Swine, result);
                break;

            case Result.Throwup:
                HandleThrowup(userContext, feedManager.Swine, result);
                break;

            case Result.Full:
                HandleFull(feedManager.Swine);
                break;
        }

        FeedResult = result;

        return Task.CompletedTask;
    }

    private void HandleFull(Swine swine)
    {
        Text.Verbatim("После недавнего инцидента с перееданием ")
            .Bold(swine.Name)
            .Verbatim(" совсем не до еды...");
    }

    private void HandleThrowup(UserContext userContext, Swine swine, FeedResult result)
    {
        swine.Weight = result.NewWeight;
        swine.WeightLosses.Add(new WeightLoss()
        {
            DateTime = result.UtcDT,
            IsThrowUp = true,
            Amount = result.Amount
        });

        switch (result.Amount)
        {
            case 0:
                Text.Bold(swine.Name)
                    .Verbatim(" уже почти стошнило, но в последний момент свин сдержал позыв и, нахмурившись, утопал обратно на своё место.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"Вес не изменился: {result.NewWeight} кг");

                var consecutiveOverfeeds = OverfeedAchievementChecker.CountConsecutiveOverfeeds(userContext, swine.SwineId);
                if (consecutiveOverfeeds != 0)
                    Text.LineBreak()
                        .Italic($"Это происшествие не нарушит ваш стрик в {consecutiveOverfeeds} {MessageTextUtils.GetDeclinatedNoun(consecutiveOverfeeds, Unit.Overfeed)}");

                break;

            default:
                Text.Verbatim("Едва глаза ")
                    .Bold(swine.Name)
                    .Verbatim(" увидели еду, всё его тело содрогнулось в рвотном позыве... Заблевав всю кормушку, изрядно исхудавший хряк грустно вернулся в глубину хлева.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"{result.OldWeight} кг - {result.AbsAmount} кг → {result.NewWeight} кг");

                break;
        }
    }

    private void HandleFeed(Swine swine, FeedResult result)
    {
        swine.Weight = result.NewWeight;
        swine.Feeds.Add(new Feed()
        {
            DateTime = result.UtcDT,
            Amount = result.Amount,
        });

        switch (result.Luck)
        {
            case < LOW_LUCK_THRESHOLD when result.Result == Result.FirstFeed:
                Text.Verbatim("К сожалению, ")
                    .Bold(swine.Name)
                    .Verbatim(" сегодня ночью приснился кошмар, поэтому он выглядит угрюмым и почти не ест...").LineBreak();
                break;

            case < LOW_LUCK_THRESHOLD when result.Result == Result.Overfeed:
                Text.Bold(swine.Name).Verbatim(", явно сытый, неохотно жуёт очередную порцию...").LineBreak();
                break;

            case > LOW_LUCK_THRESHOLD and < HIGH_LUCK_THRESHOLD when result.Result == Result.FirstFeed:
                Text.Bold(swine.Name)
                    .Verbatim(" спокойно ест из своей кормушки.").LineBreak();
                break;

            case > LOW_LUCK_THRESHOLD and < HIGH_LUCK_THRESHOLD when result.Result == Result.Overfeed:
                Text.Bold(swine.Name)
                    .Verbatim(" довольно поедает добавку.").LineBreak();
                break;

            case > HIGH_LUCK_THRESHOLD when result.Result == Result.FirstFeed:
                Text.Verbatim("Сегодня ")
                    .Bold(swine.Name)
                    .Verbatim(" проснулся с отличным аппетитом и радостно хрюкает при вашем приближении!").LineBreak();
                break;

            case > HIGH_LUCK_THRESHOLD when result.Result == Result.Overfeed:
                Text.Verbatim("Как ни в чём ни бывало, ")
                    .Bold(swine.Name)
                    .Verbatim(" налетает на новую порцию!").LineBreak();
                break;

            default:
                throw new NotSupportedException($"Unreachable code: luck is {result.Luck}, result is {result.Result.ToString()}");
        }

        Text.LineBreak()
            .Bold($"{result.OldWeight} кг + {result.Amount} кг → {result.NewWeight} кг");

        if (result.Result == Result.Overfeed)
        {
            var feedDecl = MessageTextUtils.GetDeclinatedNoun(result.RecentFeedsCount, Unit.Meal);
            Text.LineBreak()
                .Italic($"⚠ Перекорм! {result.RecentFeedsCount} {feedDecl} пищи за последние {FeedGenerator.OVERFEED_COOLDOWN} часа!");
        }
    }
}
