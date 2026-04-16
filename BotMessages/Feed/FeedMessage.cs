using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Actions.Commands;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Feed;

public class FeedMessage : BotMessage<FeedViewModel>
{
    public override void Init<T>(ILogger<T> logger, FeedViewModel viewModel)
    {
        switch (viewModel.Result.Result)
        {
            case Result.FirstFeed:
            case Result.Overfeed:
                HandleFeed(viewModel);
                break;

            case Result.Throwup:
                HandleThrowup(viewModel);
                break;

            case Result.Full:
                HandleFull(viewModel);
                break;
        }
    }

    private void HandleFull(FeedViewModel viewModel)
    {
        Text.Verbatim("После недавнего инцидента с перееданием ")
            .Bold(viewModel.SwineName)
            .Verbatim(" совсем не до еды...");
    }

    private void HandleThrowup(FeedViewModel viewModel)
    {
        switch (viewModel.Result.Amount)
        {
            case 0:
                Text.Bold(viewModel.SwineName)
                    .Verbatim(" уже почти стошнило, но в последний момент свин сдержал позыв и, нахмурившись, утопал обратно на своё место.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"Вес не изменился: {viewModel.Result.NewWeight} кг");

                if (viewModel.ConsecutiveOverfeeds != 0)
                    Text.LineBreak()
                        .Italic($"Это происшествие не нарушит ваш стрик в {viewModel.ConsecutiveOverfeeds} {MessageTextUtils.GetDeclinatedNoun(viewModel.ConsecutiveOverfeeds, Unit.Overfeed)}");

                break;

            default:
                Text.Verbatim("Едва глаза ")
                    .Bold(viewModel.SwineName)
                    .Verbatim(" увидели еду, всё его тело содрогнулось в рвотном позыве... Заблевав всю кормушку, изрядно исхудавший хряк грустно вернулся в глубину хлева.")
                    .LineBreak()
                    .LineBreak()
                    .Bold($"{viewModel.Result.OldWeight} кг - {viewModel.Result.AbsAmount} кг → {viewModel.Result.NewWeight} кг");

                break;
        }
    }

    private void HandleFeed(FeedViewModel viewModel)
    {
        switch (viewModel.Result.Luck)
        {
            case <= FeedCommand.LOW_LUCK_THRESHOLD when viewModel.Result.Result == Result.FirstFeed:
                Text.Verbatim("К сожалению, ")
                    .Bold(viewModel.SwineName)
                    .Verbatim(" сегодня ночью приснился кошмар, поэтому он выглядит угрюмым и почти не ест...").LineBreak();
                break;

            case <= FeedCommand.LOW_LUCK_THRESHOLD when viewModel.Result.Result == Result.Overfeed:
                Text.Bold(viewModel.SwineName).Verbatim(", явно сытый, неохотно жуёт очередную порцию...").LineBreak();
                break;

            case > FeedCommand.LOW_LUCK_THRESHOLD and < FeedCommand.HIGH_LUCK_THRESHOLD when viewModel.Result.Result == Result.FirstFeed:
                Text.Bold(viewModel.SwineName)
                    .Verbatim(" спокойно ест из своей кормушки.").LineBreak();
                break;

            case > FeedCommand.LOW_LUCK_THRESHOLD and < FeedCommand.HIGH_LUCK_THRESHOLD when viewModel.Result.Result == Result.Overfeed:
                Text.Bold(viewModel.SwineName)
                    .Verbatim(" довольно поедает добавку.").LineBreak();
                break;

            case >= FeedCommand.HIGH_LUCK_THRESHOLD when viewModel.Result.Result == Result.FirstFeed:
                Text.Verbatim("Сегодня ")
                    .Bold(viewModel.SwineName)
                    .Verbatim(" проснулся с отличным аппетитом и радостно хрюкает при вашем приближении!").LineBreak();
                break;

            case >= FeedCommand.HIGH_LUCK_THRESHOLD when viewModel.Result.Result == Result.Overfeed:
                Text.Verbatim("Как ни в чём ни бывало, ")
                    .Bold(viewModel.SwineName)
                    .Verbatim(" налетает на новую порцию!").LineBreak();
                break;

            default:
                throw new NotSupportedException($"Unreachable code: luck is {viewModel.Result.Luck}, result is {viewModel.Result.Result.ToString()}");
        }

        Text.LineBreak()
            .Bold($"{viewModel.Result.OldWeight} кг + {viewModel.Result.Amount} кг → {viewModel.Result.NewWeight} кг");

        if (viewModel.Result.Result == Result.Overfeed)
        {
            var feedDecl = MessageTextUtils.GetDeclinatedNoun(viewModel.Result.RecentFeedsCount, Unit.Meal);
            Text.LineBreak()
                .Italic($"⚠ Перекорм! {viewModel.Result.RecentFeedsCount} {feedDecl} пищи за последние {FeedGenerator.OVERFEED_COOLDOWN} часа!");
        }
    }
}
