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
        var value = GetValue(botMessage, swine);
        Log.Logger.Debug("CheckLevel(): value={value}, level={level}", value, levelValue);

        if (value is null)
            return CheckerResult.Break;

        if (DoesLevelApply(value.Value, levelValue))
        {
            Log.Logger.Debug("level applies");
            return IsSilentApply(botMessage, swine) ? CheckerResult.ApplySilent : CheckerResult.Apply;
        }

        Log.Logger.Debug("level does not apply");
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
            if (swine.Stats.Achievements.Where(a => a.Type == Type).Any(a => a.Value >= level.Value)) // Swine already has the current level achievement
                return false;

            var checkerResult = CheckLevel(botMessage, swine, level.Value);

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
            SwineInfoId = swine.Stats.InfoId
        };

        var lowerLevelAchievs = swine.Stats.Achievements.Where(a => a.Type == Type).ToList();
        foreach (var lowerLevelAchiev in lowerLevelAchievs)
        {
            swine.Stats.Achievements.Remove(lowerLevelAchiev);
        }

        swine.Stats.Achievements.Add(newLevelAchiev);
    }
}

