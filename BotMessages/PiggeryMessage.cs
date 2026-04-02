using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages.Start;
using SwineBot.BotMessages.Start.Actions;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class PiggeryMessage(ILogger<PiggeryMessage> logger, StartLinkBuilder linkBuilder, Config config, int userId) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        // Contains SelectedSwine if IsPrivate, contains current group swine if not
        Swine swine = null;
        if (swineId >= 0)
            swine = userContext.Swines.First(s => s.SwineId == swineId);

        var swines = userContext.Swines
            .AsNoTracking()
            .Where(s => s.OwnerId == userId).ToList();

        if (swines.Count == 0)
        {
            Text.Verbatim("Ваш свинарник пуст :(").LineBreak();
            Text.Verbatim($"Для того, чтобы завести свинку, добавьте бота в группу и отправьте {InfoCommand.NAME}").LineBreak();
            return Task.CompletedTask;
        }

        if (swines.Count == 1)
        {
            if (swine is null)
            {
                var onlySwine = swines.First();
                throw new NotSupportedException($"User [{onlySwine.OwnerId}] has one swine [{onlySwine.SwineId}], but it is not selected as private one");
            }

            Text.Verbatim("В вашем свинарнике всего один свин, ").Bold(swine.Name);

            if (IsPrivate)
            {
                var groupTitle = userContext.Groups.First(g => g.GroupId == swine.GroupId).Title;
                Text.Verbatim($" из \"{groupTitle}\"");
            }

            Text.LineBreak();
            Text.Verbatim("Для увеличения свинарника добавляйте бота в другие группы");

            return Task.CompletedTask;
        }

        if (swine is null && IsPrivate)
            Text.Verbatim("Для использования бота в личных сообщениях необходимо выбрать свина:").LineBreak().LineBreak();

        Text.Bold("Ваш свинарник:").LineBreak();

        var pairs = swines.Select(s => new
        {
            Swine = s,
            Group = userContext.Groups
            .AsNoTracking()
            .First(g => g.GroupId == s.GroupId)
        });

        int index = 0;
        foreach (var pair in pairs.OrderByDescending(p => p.Swine.Weight))
        {
            ++index;
            var caption = $"{pair.Swine.Name} из \"{pair.Group.Title}\": {pair.Swine.Weight} кг";

            Text.Verbatim($"{index}. ");

            if (!IsPrivate)
            {
                Text.Verbatim(caption).LineBreak();
                continue;
            }

            if (swine != null && pair.Swine.SwineId == swine.SwineId)
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

        if (!IsPrivate)
        {
            Text.LineBreak();
            Text.Italic("Отправьте эту команду в ").InlineMention("личном диалоге с ботом", config.Username).Italic(", чтобы кормить свинов там").LineBreak();
        }

        return Task.CompletedTask;
    }
}
