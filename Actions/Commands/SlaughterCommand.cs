using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands.Duel;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo(COMMAND_NAME, "Убить свинку \U0001f52a")]
public class SlaughterCommand(ILogger<SlaughterCommand> logger, UserContext context, IDateTimeNowProvider dtnProvider, IMessageFactory messageFactory, AchievementController achievController) : Command<SlaughterMessage, SlaughterViewModel>(messageFactory, achievController), IActionCommand
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

        // Забой свина под висящим вызовом считается отказом: свиной бог не уважает
        // тех, кто прячется за жертвенным ножом
        if (await context.GetActiveIncomingDuelRequest(swine.SwineId, dtnProvider.UtcNow) is { } pendingDuel)
        {
            await DuelDecline.ApplyAsync(context, pendingDuel, dtnProvider.UtcNow, "duel dodged by slaughter");
            logger.LogInformation("Duel request [{requestId}] closed as decline because swine [{swine}] was slaughtered", pendingDuel.RequestId, swine.SwineId);
        }

        // Дебафф записан на забиваемом свине, а он сейчас исчезнет: переносим на наследника
        var declines = swine.Declines;
        var feedStealUntil = swine.FeedStealUntil;
        var feedStealSwineId = swine.FeedStealSwineId;

        // Дуэли больше не трогают цену забоя: убийца и без того обнуляет свою массу
        var slaughteredWeight = Math.Max(0, swine.Weight - 1);
        context.Slaughters.Add(new Slaughter() { UserId = swine.OwnerId, GroupId = swine.GroupId, DateTime = dtnProvider.UtcNow, SwineWeight = slaughteredWeight, SwineName = swine.Name });

        context.Swines.Remove(swine);

        var newSwine = new Swine()
        {
            Name = (await context.Users.FirstAsync(u => u.UserId == swine.OwnerId)).FirstName,
            Weight = 1,
            GroupId = swine.GroupId,
            OwnerId = swine.OwnerId,

            // Долги предшественника переходят наследнику
            Declines = declines,
            FeedStealUntil = feedStealUntil,
            FeedStealSwineId = feedStealSwineId,
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
