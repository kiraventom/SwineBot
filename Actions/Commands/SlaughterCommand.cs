using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo(COMMAND_NAME, "Убить свинку \U0001f52a")]
public class SlaughterCommand(UserContext context, IDateTimeNowProvider dtnProvider, IMessageFactory messageFactory, AchievementController achievController) : Command<SlaughterMessage, SlaughterViewModel>(messageFactory, achievController)
{
    public const string COMMAND_NAME = "/slaughter";
    public const string CONFIRMATION = "yes";

    public const int MIN_SWINE_WEIGHT = 75;
    private const int SLAUGHTER_COOLDOWN = 24;

    protected override async Task<SlaughterViewModel> ExecuteInternal(Update update, string parameter)
    {
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);

        var lastSlaughter = await context.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .OrderByDescending(s => s.DateTime)
            .FirstOrDefaultAsync();

        var isSlaughterTooEarly = lastSlaughter is not null && (dtnProvider.UtcNow - lastSlaughter.DateTime).TotalHours < SLAUGHTER_COOLDOWN;

        if (isSlaughterTooEarly)
            return new SlaughterViewModel(swine, true, 0, true, null);

        var infoId = (await context.Infos.FirstAsync(i => i.SwineId == update.SwineId)).InfoId;
        var achievsCount = await context.Achievements.Where(s => s.SwineInfoId == infoId).CountAsync();

        var receivedConfirmation = parameter != null && string.Equals(parameter.Trim(), CONFIRMATION, StringComparison.OrdinalIgnoreCase);

        if (!receivedConfirmation)
            return new SlaughterViewModel(swine, false, achievsCount, false, null);

        var slaughteredWeight = swine.Weight - 1;
        context.Slaughters.Add(new Slaughter() { UserId = swine.OwnerId, GroupId = swine.GroupId, DateTime = dtnProvider.UtcNow, SwineWeight = slaughteredWeight, SwineName = swine.Name });

        context.Swines.Remove(swine);

        var newSwine = new Swine()
        {
            Name = (await context.Users.FirstAsync(u => u.UserId == swine.OwnerId)).FirstName,
            Weight = 1,
            GroupId = swine.GroupId,
            OwnerId = swine.OwnerId
        };

        context.Swines.Add(newSwine);
        await context.SaveChangesAsync();

        var info = new SwineInfo()
        {
            SwineId = newSwine.SwineId
        };

        context.Infos.Add(info);

        return new SlaughterViewModel(swine, false, achievsCount, true, slaughteredWeight >= MIN_SWINE_WEIGHT);
    }
}
