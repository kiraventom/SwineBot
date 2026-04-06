using Microsoft.Extensions.DependencyInjection;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public delegate AchievementChecker BuildCheckerDelegate(Type type, IReadOnlyList<AchievementLevel> levels);

public class AchievementCheckerFactory(IServiceProvider sp, IAchievementCheckerBuilders builders)
{
    public AchievementChecker Build(AchievementType type) => builders.Get(type).Build(BuildChecker);

    public IEnumerable<AchievementChecker> BuildAll()
    {
        foreach (var builder in builders.GetAll())
            yield return builder.Build(BuildChecker);
    }

    private AchievementChecker BuildChecker(Type type, IReadOnlyList<AchievementLevel> levels)
    {
        return (AchievementChecker)ActivatorUtilities.CreateInstance(sp, type, levels);
    }
}
