using SwineBot.Achievements.Effects;

namespace SwineBot.Achievements;

public record AchievementLevel(int Value, string Name, string Description, IAchievementEffect Effect);
