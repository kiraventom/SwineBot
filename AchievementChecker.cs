using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements;

public abstract class AchievementChecker(IReadOnlyCollection<AchievementLevel> values)
{
    protected IReadOnlyCollection<AchievementLevel> Levels { get; } = values;
    protected abstract AchievementType AchievementType { get; }

    protected abstract CheckerResult CheckLevel(BotMessage botMessage, Swine swine, int levelValue);

    public bool TryApply(BotMessage botMessage, Swine swine, out AchievementLevel achievementLevel)
    {
        achievementLevel = null;

        foreach (var level in Levels)
        {
            if (swine.Stats.Achievements.Where(a => a.Type == AchievementType).Any(a => a.Value >= level.Value)) // Swine already has the current level achievement
                return false;

            switch (CheckLevel(botMessage, swine, level.Value))
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
            Type = AchievementType,
            Value = levelValue,
            SwineInfoId = swine.Stats.InfoId
        };

        var lowerLevelAchievs = swine.Stats.Achievements.Where(a => a.Type == AchievementType).ToList();
        foreach (var lowerLevelAchiev in lowerLevelAchievs)
        {
            swine.Stats.Achievements.Remove(lowerLevelAchiev);
        }

        swine.Stats.Achievements.Add(newLevelAchiev);
    }
}

