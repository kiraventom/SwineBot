using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;
using SwineBot.Achievements.Checkers;
using SwineBot.Achievements.Effects;

namespace SwineBot.Achievements;

public class AchievementController
{
    private readonly ILogger _logger;
    private readonly BotMessageSender _sender;

    private readonly IReadOnlyCollection<AchievementChecker> _checkers;

    public AchievementController(ILogger logger, BotMessageSender sender)
    {
        _logger = logger;
        _sender = sender;

        _sender.BeforeMessageSend += OnBeforeMessageSend;

        List<AchievementChecker> checkers = 
        [
            new AchievementCheckerBuilder()
                .Type(AchievementType.Weight)
                .Description("Набрать {0} {1}", Unit.Kg)
                .AddLevel(100, "Сотка")
                .AddLevel(228, "Да не торч я")
                .AddLevel(420, "Ладно я торч")
                .AddLevel(690, "Кевин")
                .AddLevel(777, "А потом поебалися")
                .AddLevel(812, "Беглов")
                .AddLevel(999, "Недобор")
                .AddLevel(1000, "Речь идёт о четырёхзначных числах")
                .AddLevel(1234, "Код от домофона")
                .AddLevel(1337, "}{@k3p")
                .AddLevel(1488, "Егор Просвинин")
                .AddLevel(1580, "Вертикаль")
                .AddLevel(1703, "Круче Петра")
                .AddLevel(2000, "2K")
                .AddLevel(2007, "Вернулся")
                .AddLevel(2026, "It's me!")
                .AddLevel(5000, "А хули вы хотели?")
                .Build(),

            new AchievementCheckerBuilder()
                .Type(AchievementType.WeightGain)
                .Description("Поесть на {0} {1}", Unit.Kg)
                .AddLevel(1, "Заморил червячка")
                .AddLevel(22, "От пуза")
                .Build(),

            new AchievementCheckerBuilder()
                .Type(AchievementType.WeightLoss)
                .Description("Похудеть на {0} {1}", Unit.Kg)
                .AddLevel(-1, "И не заметил")
                .AddLevel(-20, "Серьёзный ущерб")
                .AddLevel(-40, "Жадность фраера сгубила")
                .AddLevel(-60, "he bought", new ThrowupScaleEffect(0.95))
                .AddLevel(-80, "После такого не встают", new ThrowupScaleEffect(0.85))
                .AddLevel(-100, "Булимия", new ThrowupScaleEffect(0.70))
                .Build(),

            new AchievementCheckerBuilder()
                .Type(AchievementType.Overfeed)
                .Description("Успешный перекорм {0} {1} подряд", Unit.Time)
                .AddLevel(3, "Завтрак, обед, ужин")
                .AddLevel(5, "Плюс полдник и ланч")
                .AddLevel(7, "Недельный рацион")
                .AddLevel(14, "Двухнедельный рацион", new ThrowupIgnoreChanceEffect(0.25))
                .AddLevel(21, "Трёхнедельный рацион", new ThrowupIgnoreChanceEffect(0.50))
                .AddLevel(31, "Месячная порция", new ThrowupIgnoreChanceEffect(0.75))
                .Build(),

            new AchievementCheckerBuilder()
                .Type(AchievementType.NoOverfeed)
                .Description("Без перекорма {0} {1} подряд", Unit.Time)
                .AddLevel(3, "Пивная диета")
                .AddLevel(5, "Йогуртовая диета")
                .AddLevel(7, "Яблочная диета")
                .AddLevel(14, "Водная диета")
                .AddLevel(21, "Диета Типичной Анорексички")
                .AddLevel(31, "Диета Ларисы Долиной", new OverfeedScaleModifierEffect(0.75))
                .Build(),
        ];

        _checkers = checkers;
    }

    public AchievementLevel GetLevel(Achievement achievement)
    {
        var checker = _checkers.FirstOrDefault(c => c.Type == achievement.Type);
        if (checker is null)
            return null;

        var level = checker.GetLevel(achievement);
        return level;
    }

    private async Task OnBeforeMessageSend(UserContext userContext, ChatId chatId, int userId, BotMessage message)
    {
        if (message is AchievementMessage)
            return;

        var swine = userContext.Swines
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
            .Include(s => s.Feeds)
            .Include(s => s.WeightLosses)
            .First(s => s.OwnerId == userId);

        foreach (var checker in _checkers)
        {
            if (checker.TryApply(message, swine, out var achievementLevel))
            {
                await _sender.Send(userContext, chatId, userId, new AchievementMessage(_logger, achievementLevel));
            }
        }
    }
}
