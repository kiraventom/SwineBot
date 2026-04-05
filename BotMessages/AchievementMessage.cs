using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class AchievementMessage(ILogger<AchievementMessage> logger, UserContext context, AchievementLevel achievementLevel) : BotMessage(logger)
{
    protected override Task InitInternal(Update update)
    {
        var swine = context.Swines.First(s => s.SwineId == update.SwineId);
        
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

