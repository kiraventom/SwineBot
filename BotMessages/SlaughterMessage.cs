using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Actions.Commands;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class SlaughterMessage(ILogger<SlaughterMessage> Logger, IDateTimeNowProvider dtnProvider, string confirmation) : BotMessage(Logger)
{
    private const string CONFIRMATION = "yes";
    private const int SLAUGHTER_COOLDOWN = 24;

    public const int MIN_SWINE_WEIGHT = 75;

    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        var swine = userContext.Swines.First(s => s.SwineId == swineId);

        var lastSlaughter = userContext.Slaughters
            .Where(s => s.UserId == swine.OwnerId)
            .OrderByDescending(s => s.DateTime)
            .FirstOrDefault();

        if (lastSlaughter is not null && (dtnProvider.UtcNow - lastSlaughter.DateTime).TotalHours < SLAUGHTER_COOLDOWN)
        {
            Text.Italic("Нельзя марать руки в крови так часто.");
            return Task.CompletedTask;
        }

        var infoId = userContext.Infos.First(i => i.SwineId == swineId).InfoId;
        var achievsCount = userContext.Achievements.Where(s => s.SwineInfoId == infoId).Count();

        if (confirmation == null || !string.Equals(confirmation.Trim(), CONFIRMATION, StringComparison.OrdinalIgnoreCase))
        {
            Text.Italic("Вы собираетесь").Verbatim(" ").Underline("убить").Verbatim(" ").Italic("вашу свинку ").Bold(swine.Name).LineBreak();
            Text.Italic("Вы потеряете ").Bold(swine.Weight).Italic($" {MessageTextUtils.GetDeclinatedNoun(swine.Weight, Unit.Kg)} сальца");
            if (achievsCount != 0)
                Text.Italic(", ").Bold(achievsCount).Italic($" {MessageTextUtils.GetDeclinatedNoun(achievsCount, Unit.Achievement)}");

            Text.Italic(" и верного друга.").LineBreak().LineBreak();

            Text.Bold(swine.Name).Verbatim(" радостно смотрит на вас, думая, что вы принесли корм.").LineBreak();
            Text.Italic("Чтобы убить ").Bold(swine.Name).Italic(", отправьте ")
               .Monospace($"{SlaughterCommand.COMMAND_NAME} {CONFIRMATION}");

            return Task.CompletedTask;
        }

        var slaughteredWeight = swine.Weight - 1;
        userContext.Slaughters.Add(new Slaughter() { UserId = swine.OwnerId, GroupId = swine.GroupId, DateTime = dtnProvider.UtcNow, SwineWeight = slaughteredWeight, SwineName = swine.Name });

        Text.Bold(swine.Name).Italic(" жалобно визжит и испускает последний вздох.").LineBreak();

        userContext.Swines.Remove(swine);
        userContext.SaveChanges();

        var newSwine = new Swine()
        {
            Name = userContext.Users.First(u => u.UserId == swine.OwnerId).FirstName,
            Weight = 1,
            GroupId = swine.GroupId,
            OwnerId = swine.OwnerId
        };

        userContext.Swines.Add(newSwine);
        userContext.SaveChanges();

        var info = new SwineInfo()
        {
            SwineId = newSwine.SwineId
        };

        userContext.Infos.Add(info);
        userContext.SaveChanges();

        if (slaughteredWeight >= MIN_SWINE_WEIGHT)
            Text.Italic("Теперь ваши будущие свинки будут расти быстрее...");
        else
            Text.Italic("Это жестокое убийство не принесло никакого эффекта.");

        return Task.CompletedTask;
    }
}
