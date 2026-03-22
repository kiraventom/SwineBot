using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public abstract class AchievementChecker(IReadOnlyCollection<AchievementLevel> values)
{
    public abstract AchievementType Type { get; }
    protected IReadOnlyCollection<AchievementLevel> Levels { get; } = values;

    protected abstract int? GetValue(BotMessage botMessage, Swine swine);

    protected abstract bool DoesLevelApply(int value, int level);

    protected abstract bool IsSilentApply(BotMessage botMessage, Swine swine);

    private CheckerResult CheckLevel(BotMessage botMessage, Swine swine, int levelValue)
    {
        // Swine already has the achievement of that of bigger level
        var higherLevelAchievement = swine.Info.Achievements
            .Where(a => a.Type == this.Type)
            .FirstOrDefault(a => DoesLevelApply(a.Value, levelValue));

        if (higherLevelAchievement != null)
        {
            Log.Logger.Debug("Already has achievement of higher level: id={id}", higherLevelAchievement.AchievementId);
            return CheckerResult.Break;
        }

        var value = GetValue(botMessage, swine);

        if (value is null)
        {
            Log.Logger.Warning("GetValue returned null");
            return CheckerResult.Break;
        }

        if (DoesLevelApply(value.Value, levelValue))
        {
            return IsSilentApply(botMessage, swine) ? CheckerResult.ApplySilent : CheckerResult.Apply;
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

    public bool TryApply(BotMessage botMessage, Swine swine, out AchievementLevel achievementLevel)
    {
        achievementLevel = null;

        foreach (var level in Levels)
        {
            var checkerResult = CheckLevel(botMessage, swine, level.Value);
            Log.Logger.Debug("Checking {checker}, level {level}, result {result}", this.Type.ToString(), level.Value.ToString(), checkerResult.ToString());

            switch (checkerResult)
            {
                case CheckerResult.Apply:
                    Apply(swine, level.Value);
                    achievementLevel = level;
                    return true;

                case CheckerResult.ApplySilent:
                    Apply(swine, level.Value);
                    return false;

                case CheckerResult.Break:
                    return false;

                case CheckerResult.Continue:
                    continue;
            }
        }

        return false;
    }

    private void Apply(Swine swine, int levelValue)
    {
        var newLevelAchiev = new Achievement()
        {
            Type = Type,
            DateTime = DateTime.Now.ToUniversalTime(),
            Value = levelValue,
            SwineInfoId = swine.Info.InfoId
        };

        var lowerLevelAchievs = swine.Info.Achievements.Where(a => a.Type == Type).ToList();
        foreach (var lowerLevelAchiev in lowerLevelAchievs)
        {
            swine.Info.Achievements.Remove(lowerLevelAchiev);
        }

        swine.Info.Achievements.Add(newLevelAchiev);
    }
}

