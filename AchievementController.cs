using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot.Types;

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

        List<AchievementChecker> checkers = 
        [
            new AchievementCheckerBuilder()
                .Type(AchievementType.Weight)
                .Description("Набрать {0} килограмм")
                .AddLevel(100, "Сотка")
                .AddLevel(228, "Да не торч я")
                .AddLevel(420, "Ладно я торч")
                .AddLevel(690, "Кевин")
                .AddLevel(812, "Беглов")
                .AddLevel(999, "Недобор")
                .AddLevel(1000, "Речь идёт о четырёхзначных числах")
                .Build(),
        ];

        _checkers = checkers;
    }

    public async void OnBeforeMessageSend(UserContext userContext, ChatId chatId, int userId, BotMessage message)
    {
        if (message is AchievementMessage)
            return;

        var swine = userContext.Swines
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
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
