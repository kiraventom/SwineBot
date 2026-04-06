using Microsoft.Extensions.DependencyInjection;

namespace SwineBot.Achievements.Checkers;

public interface IAchievementCheckerBuilderFactory
{
    AchievementCheckerBuilder Create();
}

public class AchievementCheckerBuilderFactory(IServiceProvider sp) : IAchievementCheckerBuilderFactory
{
    public AchievementCheckerBuilder Create()
    {
        return ActivatorUtilities.CreateInstance<AchievementCheckerBuilder>(sp);
    }
}
