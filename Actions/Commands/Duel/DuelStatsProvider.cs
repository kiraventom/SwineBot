using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwineBot.Achievements;
using SwineBot.Achievements.Checkers;
using SwineBot.Model;

namespace SwineBot.Actions.Commands.Duel;

/// <summary>
/// Считает боевые характеристики свина: возраст, пережитые в беседе забои,
/// очки ачивок (сумма уровней) и славу. Источник бонуса выбирает DuelBalance.ChanceSource,
/// но считаем всё сразу — так проще переключать режимы в тестах.
/// </summary>
public class DuelStatsProvider(UserContext context, AchievementCheckerFactory checkerFactory, IReadOnlyDictionary<AchievementType, AchievementData> achievementDatas)
{
    public async Task<DuelCombatStats> GetAsync(Swine swine, DateTime utcNow)
    {
        var firstFeed = await context.Feeds
            .Where(f => f.SwineId == swine.SwineId)
            .OrderBy(f => f.DateTime)
            .Select(f => (DateTime?)f.DateTime)
            .FirstOrDefaultAsync();

        // Свин без кормёжек ещё не жил: возраст и свидетели считаются с первого покорма
        if (firstFeed is null)
            return new DuelCombatStats(0, 0, await GetAchievementPointsAsync(swine), swine.Glory);

        var ageDays = Math.Max(0, (int)(utcNow - firstFeed.Value).TotalDays);

        var witnessedKills = swine.GroupId is { } groupId
            ? await context.Slaughters.CountAsync(s => s.GroupId == groupId && s.DateTime > firstFeed.Value)
            : 0;

        return new DuelCombatStats(ageDays, witnessedKills, await GetAchievementPointsAsync(swine), swine.Glory);
    }

    /// <summary>Очки ачивок: сумма номеров уровней (1 — низший).</summary>
    private async Task<int> GetAchievementPointsAsync(Swine swine)
    {
        var info = await context.Infos.FirstOrDefaultAsync(i => i.SwineId == swine.SwineId);
        if (info is null)
            return 0;

        var achievements = await context.Achievements
            .Where(a => a.SwineInfoId == info.InfoId)
            .ToListAsync();

        var points = 0;
        foreach (var achievement in achievements)
        {
            var checker = checkerFactory.Build(achievement.Type);
            var level = checker.GetLevel(achievement);
            if (level is not null)
                points += achievementDatas[achievement.Type].GetLevelIndex(level) + 1;
        }

        return points;
    }
}
