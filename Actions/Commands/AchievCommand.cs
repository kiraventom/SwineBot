using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/achiev", "Посмотреть достижения своего свина \U0001F3C6")]
public class AchievCommand(ILogger<AchievCommand> logger, UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<AchievementsMessage, AchievsViewModel>(messageFactory, achievController)
{
    protected override async Task<AchievsViewModel> ExecuteInternal(Update update, string parameter)
    {
        var swine = await context.Swines.FirstAsync(s => s.SwineId == update.SwineId);
        var infoId = (await context.Infos.FirstAsync(i => i.SwineId == update.SwineId)).InfoId;

        var achievs = context.Achievements.Where(a => a.SwineInfoId == infoId).OrderByDescending(a => a.DateTime);

        var datedLevels = new List<DatedAchievementLevel>();
        foreach (var achiev in achievs)
        {
            var level = AchievController.GetLevel(achiev);
            if (level is null)
            {
                logger.LogError("Level for achievement {type} with value {value} was not found", achiev.Type.ToString(), achiev.Value);
                continue;
            }

            datedLevels.Add(new DatedAchievementLevel(achiev.DateTime, level));
        }

        return new AchievsViewModel(swine.Name, datedLevels);
    }
}

