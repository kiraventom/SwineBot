using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public interface IAchievementChecker
{
    AchievementType Type { get; }
    AchievementLevel GetLevel(Achievement achievement);
    Task<AchievementLevel> TryApply(BotMessage botMessage, int swineId);
}
 
public abstract class AchievementChecker(ILogger<AchievementChecker> logger, IDateTimeNowProvider dtnProvider, UserContext context, IReadOnlyCollection<AchievementLevel> values) : IAchievementChecker
{
    public abstract AchievementType Type { get; }
    protected IReadOnlyCollection<AchievementLevel> Levels { get; } = values;

    protected abstract Task<int?> GetValue(BotMessage botMessage, UserContext context, int swineId);

    protected abstract bool DoesLevelApply(int value, int level);

    private async Task<CheckerResult> CheckLevel(BotMessage botMessage, int swineId, int levelValue)
    {
        var infoId = context.Infos.First(i => i.SwineId == swineId).InfoId;
        // Swine already has the achievement of that of bigger level
        var higherLevelAchievement = context.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == this.Type)
            .AsEnumerable()
            .FirstOrDefault(a => DoesLevelApply(a.Value, levelValue));

        if (higherLevelAchievement != null)
        {
            logger.LogDebug("Already has achievement of higher level: id={id}", higherLevelAchievement.AchievementId);
            return CheckerResult.Break;
        }

        var value = await GetValue(botMessage, context, swineId);

        if (value is null)
            return CheckerResult.Break;

        if (DoesLevelApply(value.Value, levelValue))
        {
            return CheckerResult.Apply;
        }

        return CheckerResult.Continue;
    }

    public AchievementLevel GetLevel(Achievement achievement)
    {
        foreach (var level in Levels)
        {
            if (DoesLevelApply(achievement.Value, level.Value))
                return level;
        }

        return null;
    }

    public async Task<AchievementLevel> TryApply(BotMessage botMessage, int swineId)
    {
        var swine = context.Swines.First(s => s.SwineId == swineId);

        foreach (var level in Levels)
        {
            var checkerResult = await CheckLevel(botMessage, swineId, level.Value);
            logger.LogDebug("Checked {checker}, level {level}, result {result}", this.Type.ToString(), level.Value.ToString(), checkerResult.ToString());

            switch (checkerResult)
            {
                case CheckerResult.Apply:
                    await Apply(swineId, level.Value);
                    return level;

                case CheckerResult.Break:
                    return null;

                case CheckerResult.Continue:
                    continue;
            }
        }

        return null;
    }

    private async Task Apply(int swineId, int levelValue)
    {
        var infoId = context.Infos.First(i => i.SwineId == swineId).InfoId;
        var newLevelAchiev = new Achievement()
        {
            Type = Type,
            DateTime = dtnProvider.UtcNow,
            Value = levelValue,
            SwineInfoId = infoId
        };

        var lowerLevelAchievs = await context.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == Type)
            .ToListAsync();

        foreach (var lowerLevelAchiev in lowerLevelAchievs)
            context.Achievements.Remove(lowerLevelAchiev);

        context.Achievements.Add(newLevelAchiev);
    }
}

