using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public interface IAchievementCheckerFactory
{
    T Create<T>(IReadOnlyCollection<AchievementLevel> levels) where T : IAchievementChecker;
}

public class AchievementCheckerFactory(IServiceProvider sp) : IAchievementCheckerFactory
{
    public T Create<T>(IReadOnlyCollection<AchievementLevel> levels) where T : IAchievementChecker => ActivatorUtilities.CreateInstance<T>(sp, levels);
}

public interface IAchievementChecker
{
    AchievementType Type { get; }
    AchievementLevel GetLevel(Achievement achievement);
    bool TryApply(BotMessage botMessage, UserContext context, int swineId, out AchievementLevel achievementLevel);
}
 
public abstract class AchievementChecker(ILogger<AchievementChecker> Logger, IReadOnlyCollection<AchievementLevel> values) : IAchievementChecker
{
    public abstract AchievementType Type { get; }
    protected IReadOnlyCollection<AchievementLevel> Levels { get; } = values;

    protected abstract int? GetValue(BotMessage botMessage, UserContext context, int swineId);

    protected abstract bool DoesLevelApply(int value, int level);

    private CheckerResult CheckLevel(BotMessage botMessage, UserContext userContext, int swineId, int levelValue)
    {
        var infoId = userContext.Infos.First(i => i.SwineId == swineId).InfoId;
        // Swine already has the achievement of that of bigger level
        var higherLevelAchievement = userContext.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == this.Type)
            .AsEnumerable()
            .FirstOrDefault(a => DoesLevelApply(a.Value, levelValue));

        if (higherLevelAchievement != null)
        {
            Logger.LogDebug("Already has achievement of higher level: id={id}", higherLevelAchievement.AchievementId);
            return CheckerResult.Break;
        }

        var value = GetValue(botMessage, userContext, swineId);

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

    public bool TryApply(BotMessage botMessage, UserContext context, int swineId, out AchievementLevel achievementLevel)
    {
        achievementLevel = null;

        var swine = context.Swines.First(s => s.SwineId == swineId);

        foreach (var level in Levels)
        {
            var checkerResult = CheckLevel(botMessage, context, swineId, level.Value);
            Logger.LogDebug("Checking {checker}, level {level}, result {result}", this.Type.ToString(), level.Value.ToString(), checkerResult.ToString());

            switch (checkerResult)
            {
                case CheckerResult.Apply:
                    Apply(context, swineId, level.Value);
                    achievementLevel = level;
                    return true;

                case CheckerResult.Break:
                    return false;

                case CheckerResult.Continue:
                    continue;
            }
        }

        return false;
    }

    private void Apply(UserContext context, int swineId, int levelValue)
    {
        var infoId = context.Infos.First(i => i.SwineId == swineId).InfoId;
        var newLevelAchiev = new Achievement()
        {
            Type = Type,
            DateTime = DateTime.Now.ToUniversalTime(),
            Value = levelValue,
            SwineInfoId = infoId
        };

        var lowerLevelAchievs = context.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == Type)
            .ToList();

        foreach (var lowerLevelAchiev in lowerLevelAchievs)
            context.Achievements.Remove(lowerLevelAchiev);

        context.Achievements.Add(newLevelAchiev);
    }
}

