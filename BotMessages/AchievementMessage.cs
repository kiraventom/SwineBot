using Serilog;
using SwineBot.Achievements;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class AchievementMessage(ILogger logger, AchievementLevel achievementLevel) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var swine = userContext.Swines.First(s => s.OwnerId == userId);
        
        Text.Verbatim("🏆")
            .Bold(swine.Name)
            .Verbatim(" получает достижение \"")
            .Bold(achievementLevel.Name)
            .Verbatim("\"! 🏆")
            .LineBreak().LineBreak()
            .Italic(achievementLevel.Description);

        return Task.CompletedTask;
    }
}

