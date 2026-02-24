using SwineBot.Achievements.Effects;
using SwineBot.Model;

namespace SwineBot.Achievements;

public record AchievementLevel(int Value, string Name, string Description, IAchievementEffect Effect);
