using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Achievements.Checkers;

public interface IAchievementChecker
{
    AchievementType Type { get; }
    AchievementLevel GetLevel(Achievement achievement);
    Task<AchievementLevel> TryApply(ViewModel viewModel, int infoId, int swineId);
}
 
public abstract class AchievementChecker(ILogger<AchievementChecker> logger, IDateTimeNowProvider dtnProvider, UserContext context) : IAchievementChecker
{
    public abstract AchievementType Type { get; }
    protected IReadOnlyCollection<AchievementLevel> Levels { get; private set; }
    protected UserContext Context { get; } = context;

    private bool IsArchived { get; set; }

    protected abstract Task<int?> GetValue(ViewModel viewModel, int swineId);

    protected abstract bool DoesLevelApply(int value, int level);

    public void Init(IReadOnlyCollection<AchievementLevel> levels, bool isArchived)
    {
        Levels = levels;
        IsArchived = isArchived;
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

    public async Task<AchievementLevel> TryApply(ViewModel viewModel, int infoId, int swineId)
    {
        if (IsArchived)
        {
            logger.LogDebug("Checker {type} is archived, skipping", this.Type.ToString());
            return null;
        }

        foreach (var level in Levels)
        {
            var checkerResult = await CheckLevel(viewModel, infoId, swineId, level.Value);
            logger.LogDebug("Checked {checker}, level {level}, result {result}", this.Type.ToString(), level.Value.ToString(), checkerResult.ToString());

            switch (checkerResult)
            {
                case CheckerResult.Apply:
                    await Apply(swineId, level.Value);
                    return level;

                case CheckerResult.Break:
                    return null;

                case CheckerResult.Continue:
                    continue;
            }
        }

        return null;
    }

    private async Task<CheckerResult> CheckLevel(ViewModel viewModel, int infoId, int swineId, int levelValue)
    {
        // Swine already has the achievement of that of bigger level
        var higherLevelAchievement = Context.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == this.Type)
            .AsEnumerable()
            .FirstOrDefault(a => DoesLevelApply(a.Value, levelValue));

        if (higherLevelAchievement != null)
        {
            logger.LogDebug("Already has achievement of higher level: id={id}", higherLevelAchievement.AchievementId);
            return CheckerResult.Break;
        }

        var value = await GetValue(viewModel, swineId);

        if (value is null)
            return CheckerResult.Break;

        if (DoesLevelApply(value.Value, levelValue))
        {
            return CheckerResult.Apply;
        }

        return CheckerResult.Continue;
    }

    private async Task Apply(int swineId, int levelValue)
    {
        var infoId = (await Context.Infos.FirstAsync(i => i.SwineId == swineId)).InfoId;
        var newLevelAchiev = new Achievement()
        {
            Type = Type,
            DateTime = dtnProvider.UtcNow,
            Value = levelValue,
            SwineInfoId = infoId
        };

        var lowerLevelAchievs = await Context.Achievements
            .Where(a => a.SwineInfoId == infoId)
            .Where(a => a.Type == Type)
            .ToListAsync();

        foreach (var lowerLevelAchiev in lowerLevelAchievs)
            Context.Achievements.Remove(lowerLevelAchiev);

        Context.Achievements.Add(newLevelAchiev);
    }
}

