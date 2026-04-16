using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class AchievementData
{
    private readonly List<AchievementLevel> _levels;

    public AchievementType Type { get; }
    public IReadOnlyList<AchievementLevel> Levels => _levels;
    public bool IsArchived { get; }

    public AchievementData(AchievementType type, List<AchievementLevel> levels, bool isArchived)
    {
        Type = type;
        _levels = levels;
        IsArchived = isArchived;
    }

    public int GetLevelIndex(AchievementLevel level) => _levels.Count - _levels.IndexOf(level);
}

