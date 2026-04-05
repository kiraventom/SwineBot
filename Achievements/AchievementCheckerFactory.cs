using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class AchievementCheckerFactory(IServiceProvider sp, IAchievementCheckerBuilders builders)
{
    public AchievementChecker Build(AchievementType type) => builders.Get(type).Build(sp);

    public IEnumerable<AchievementChecker> BuildAll()
    {
        foreach (var builder in builders.GetAll())
            yield return builder.Build(sp);
    }
}
