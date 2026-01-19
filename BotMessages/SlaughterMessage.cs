using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Actions.Commands;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class SlaughterMessage(ILogger logger, string confirmation) : BotMessage(logger)
{
    private const string CONFIRMATION = "yes";
    private const int SLAUGHTER_COOLDOWN = 24;

    protected override Task InitInternal(UserContext userContext, int userId)
    {
        var lastSlaughter = userContext.Slaughters
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateTime)
            .FirstOrDefault();

        if (lastSlaughter is not null && (DateTime.UtcNow - lastSlaughter.DateTime).TotalHours < SLAUGHTER_COOLDOWN)
        {
            Text.Italic("Нельзя марать руки в крови так часто.");
            return Task.CompletedTask;
        }

        var swine = userContext.Swines.First(s => s.OwnerId == userId);
        var achievsCount = userContext.Achievements.Where(s => s.SwineInfoId == swine.StatsId).Count();

        if (confirmation == null || !string.Equals(confirmation.Trim(), CONFIRMATION, StringComparison.OrdinalIgnoreCase))
        {
            Text.Italic("Вы собираетесь").Verbatim(" ").Underline("убить").Verbatim(" ").Italic("вашу свинку ").Bold(swine.Name).LineBreak();
            Text.Italic("Вы потеряете ").Bold(swine.Weight).Italic(" кг сальца");
            if (achievsCount != 0)
                Text.Italic(", ").Bold(achievsCount).Italic(" достижений");

            Text.Italic(" и верного друга.").LineBreak().LineBreak();

            Text.Bold(swine.Name).Verbatim(" радостно смотрит на вас, думая, что вы принесли корм.").LineBreak();
            Text.Italic("Чтобы убить ").Bold(swine.Name).Italic(", отправьте ")
               .Monospace($"{SlaughterCommand.COMMAND_NAME} {CONFIRMATION}");

            return Task.CompletedTask;
        }

        var slaughteredWeight = swine.Weight - 1;
        userContext.Slaughters.Add(new Slaughter() { UserId = userId, DateTime = DateTime.UtcNow, SwineWeight = slaughteredWeight, SwineName = swine.Name });

        Text.Bold(swine.Name).Italic(" жалобно визжит и испускает последний вздох.").LineBreak();
        userContext.Swines.Remove(swine);

        var newSwine = new Swine()
        {
            Name = userContext.Users.First(u => u.UserId == userId).FirstName,
            Stats = new(),
            Weight = 1,
            OwnerId = userId
        };

        userContext.Swines.Add(newSwine);

        if (slaughteredWeight > 0)
            Text.Italic("Теперь ваши будущие свинки будут расти быстрее...");
        else
            Text.Italic("Это жестокое убийство не принесло никакого эффекта.");

        return Task.CompletedTask;
    }
}
