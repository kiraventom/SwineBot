using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Achievements.Checkers;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwineBot.Achievements;

public class AchievementController(ILogger<AchievementController> logger, IMessageFactory messageFactory, UserContext context, AchievementCheckerFactory checkerFactory, IReadOnlyDictionary<AchievementType, AchievementData> achievementDatas)
{
    public AchievementLevel GetLevel(Achievement achievement)
    {
        var checker = checkerFactory.Build(achievement.Type);
        var level = checker.GetLevel(achievement);
        return level;
    }

    public async Task<IReadOnlyCollection<AchievementMessage>> GetAchievMessages(int swineId, ViewModel viewModel)
    {
        var swine = await context.Swines.FirstOrDefaultAsync(s => s.SwineId == swineId);
        if (swine is null) // If swine was slaughtered
            return [];

        var swineName = swine.Name;
        var infoId = (await context.Infos.FirstAsync(i => i.SwineId == swineId)).InfoId;

        List<AchievementMessage> messages = [];

        foreach (var checker in checkerFactory.BuildAll())
        {
            AchievementLevel level;
            try
            {
                level = await checker.TryApply(viewModel, infoId, swineId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to try apply checker {checker}", checker.Type.ToString());
                throw;
            }

            if (level is not null)
            {
                var data = achievementDatas[checker.Type];
                var levelIndex = data.GetLevelIndex(level);
                var levelsCount = data.Levels.Count;

                var achievViewModel = new AchievementViewModel(swineName, level, levelIndex, levelsCount);
                messages.Add(messageFactory.Create<AchievementMessage, AchievementViewModel>(achievViewModel));
            }
        }

        return messages;
    }
}
