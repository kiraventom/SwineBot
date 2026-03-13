using Serilog;
using SwineBot.Achievements;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class AchievementMessage(ILogger logger, AchievementLevel achievementLevel) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines.First(s => s.SwineId == swineId);
        
        Text.Verbatim("🏆")
            .Bold(swine.Name)
            .Verbatim(" получает достижение \"")
            .Bold(achievementLevel.Name)
            .Verbatim("\"! 🏆")
            .LineBreak().LineBreak()
            .Italic(achievementLevel.Description);

        if (achievementLevel.Effect != null)
        {
            Text
                .LineBreak().LineBreak()
                .Verbatim("✨ Новый эффект: ")
                .Italic(achievementLevel.Effect.Description)
                .Verbatim(" ✨");
        }

        return Task.CompletedTask;
    }
}

