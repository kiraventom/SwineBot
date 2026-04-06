using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Achievements.Checkers;
using SwineBot.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SwineBot.Achievements;

public class AchievementController(IMessageFactory messageFactory, UserContext context, AchievementCheckerFactory checkerFactory)
{
    public AchievementLevel GetLevel(Achievement achievement)
    {
        var checker = checkerFactory.Build(achievement.Type);
        var level = checker.GetLevel(achievement);
        return level;
    }

    public async Task<IReadOnlyCollection<AchievementMessage>> GetAchievMessages(int swineId, ViewModel viewModel)
    {
        var swineName = (await context.Swines.FirstAsync(s => s.SwineId == swineId)).Name;
        var infoId = (await context.Infos.FirstAsync(i => i.SwineId == swineId)).InfoId;

        List<AchievementMessage> messages = [];

        foreach (var checker in checkerFactory.BuildAll())
        {
            var level = await checker.TryApply(viewModel, infoId, swineId);
            if (level is not null)
            {
                var achievViewModel = new AchievementViewModel(swineName, level);
                messages.Add(messageFactory.Create<AchievementMessage, AchievementViewModel>(achievViewModel));
            }
        }

        return messages;
    }
}
