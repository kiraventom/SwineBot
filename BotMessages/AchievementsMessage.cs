using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class AchievementsMessage(ILogger<AchievementsMessage> Logger, IAchievementController achievController) : BotMessage(Logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines
            .Include(s => s.Info).ThenInclude(s => s.Achievements)
            .FirstOrDefault(s => s.SwineId == swineId);

        if (swine.Info.Achievements.Count == 0)
        {
            Text.Italic("У ").Bold(swine.Name).Italic(" пока нет достижений :(").LineBreak();
            return Task.CompletedTask;
        }

        Text.Bold("Достижения ").Bold(swine.Name).Bold(":").LineBreak().LineBreak();

        int index = 0;
        foreach (var achiev in swine.Info.Achievements.OrderByDescending(a => a.DateTime))
        {
            WriteAchievement(achiev, index);
            ++index;
        }

        return Task.CompletedTask;
    }

    private void WriteAchievement(Achievement achiev, int index)
    {
        var level = achievController.GetLevel(achiev);
        if (level is null)
        {
            Logger.LogError("Level for achievement {type} with value {value} was not found", achiev.Type.ToString(), achiev.Value);
            return;
        }

        Text.Verbatim(index + 1).Verbatim(". ").Bold(level.Name).LineBreak();
        Text.Tab(text =>
        {
            Text.Italic(level.Description).LineBreak();
            Text.Verbatim("Получено ").Monospace(achiev.DateTime.ToString("d MMMM yyyy", Common.RuCulture)).LineBreak();

            if (level.Effect != null)
                Text.Verbatim("Эффект: ").Italic(level.Effect.Description).LineBreak();

            Text.LineBreak();
        });
    }
}




