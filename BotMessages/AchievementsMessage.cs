using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Achievements;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class AchievementsMessage(ILogger logger, AchievementController achievController) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines
            .Include(s => s.Stats).ThenInclude(s => s.Achievements)
            .FirstOrDefault(s => s.SwineId == swineId);

        if (swine.Stats.Achievements.Count == 0)
        {
            Text.Italic("У ").Bold(swine.Name).Italic(" пока нет достижений :(").LineBreak();
            return Task.CompletedTask;
        }

        if (swine.Stats.Achievements.Count == 1)
        {
            Text.Bold("Достижение ").Bold(swine.Name).Bold(":").LineBreak();
            WriteAchievement(swine.Stats.Achievements.First());
            return Task.CompletedTask;
        }

        Text.Bold("Достижения ").Bold(swine.Name).Bold(":").LineBreak();

        int index = 0;
        foreach (var achiev in swine.Stats.Achievements.OrderByDescending(a => a.DateTime))
        {
            WriteAchievement(achiev, index);
            ++index;
        }

        return Task.CompletedTask;
    }

    private void WriteAchievement(Achievement achiev, int index = -1)
    {
        var level = achievController.GetLevel(achiev);
        if (level is null)
        {
            Logger.Error("Level for achievement {type} with value {value} was not found", achiev.Type.ToString(), achiev.Value);
            return;
        }

        if (index > -1)
            Text.Bold(index + 1).Bold(". ");

        Text.Bold(level.Name).LineBreak();
        Text.Verbatim(level.Description).LineBreak();
        Text.Verbatim("Получено ").Verbatim(achiev.DateTime.ToString("d MMMM yyyy", Common.RuCulture)).LineBreak();

        if (level.Effect != null)
            Text.Italic(level.Effect.Description).LineBreak();

        Text.LineBreak();
    }
}




