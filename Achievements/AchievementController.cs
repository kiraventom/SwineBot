using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Achievements.Checkers;

namespace SwineBot.Achievements;

public class AchievementController(IMessageFactory messageFactory, AchievementCheckerFactory checkerFactory)
{
    public AchievementLevel GetLevel(Achievement achievement)
    {
        var checker = checkerFactory.Build(achievement.Type);
        var level = checker.GetLevel(achievement);
        return level;
    }

    public async IAsyncEnumerable<AchievementMessage> GetAchievMessages(int swineId, BotMessage message)
    {
        if (message is AchievementMessage)
            yield break;

        foreach (var checker in checkerFactory.BuildAll())
        {
            var level = await checker.TryApply(message, swineId);
            if (level is not null)
                yield return messageFactory.Create<AchievementMessage>(level);
        }
    }
}
