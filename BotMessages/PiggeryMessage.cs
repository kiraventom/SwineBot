using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages.Start;
using SwineBot.BotMessages.Start.Actions;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class PiggeryMessage(ILogger<PiggeryMessage> logger, UserContext context, StartLinkBuilder linkBuilder, Config config) : BotMessage(logger)
{
    protected override async Task InitInternal(Update update)
    {
        Swine selectedSwine = null;
        if (update.SwineId >= 0)
            selectedSwine = context.Swines.First(s => s.SwineId == update.SwineId);

        var swines = await context.Swines
            .AsNoTracking()
            .Where(s => s.OwnerId == update.UserId)
            .ToListAsync();

        if (swines.Count == 0)
        {
            Text.Verbatim("Ваш свинарник пуст :(").LineBreak();
            Text.Verbatim($"Для того, чтобы завести свинку, добавьте бота в группу и отправьте {InfoCommand.NAME}").LineBreak();
            return;
        }

        if (swines.Count == 1)
        {
            if (selectedSwine is null)
            {
                var onlySwine = swines.First();
                throw new NotSupportedException($"User [{onlySwine.OwnerId}] has one swine [{onlySwine.SwineId}], but it is not selected as private one");
            }

            Text.Verbatim("В вашем свинарнике всего один свин, ").Bold(selectedSwine.Name);

            if (update.IsPrivateChat)
            {
                var groupTitle = context.Groups.First(g => g.GroupId == selectedSwine.GroupId).Title;
                Text.Verbatim($" из \"{groupTitle}\"");
            }

            Text.LineBreak();
            Text.Verbatim("Для увеличения свинарника добавляйте бота в другие группы");
        }

        if (selectedSwine is null && update.IsPrivateChat)
            Text.Verbatim("Для использования бота в личных сообщениях необходимо выбрать свина:").LineBreak().LineBreak();

        Text.Bold("Ваш свинарник:").LineBreak();

        var pairs = swines.Select(s => new
        {
            Swine = s,
            Group = context.Groups
            .AsNoTracking()
            .First(g => g.GroupId == s.GroupId)
        });

        int index = 0;
        foreach (var pair in pairs.OrderByDescending(p => p.Swine.Weight))
        {
            ++index;
            var caption = $"{pair.Swine.Name} из \"{pair.Group.Title}\": {pair.Swine.Weight} кг";

            Text.Verbatim($"{index}. ");

            if (!update.IsPrivateChat)
            {
                Text.Verbatim(caption).LineBreak();
                continue;
            }

            if (selectedSwine != null && pair.Swine.SwineId == selectedSwine.SwineId)
            {
                Text.Underline(caption);
            }
            else
            {
                var selectSwineAction = new SetSwineAction() { SwineId = pair.Swine.SwineId };
                Text.InlineUrl(caption, linkBuilder.Build(selectSwineAction));
            }

            Text.LineBreak();
        }

        if (!update.IsPrivateChat)
        {
            Text.LineBreak();
            Text.Italic("Отправьте эту команду в ").InlineMention("личном диалоге с ботом", config.Username).Italic(", чтобы кормить свинов там").LineBreak();
        }
    }
}
