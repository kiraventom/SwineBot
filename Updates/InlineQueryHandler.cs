using Microsoft.Extensions.Logging;
using SwineBot.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.EntityFrameworkCore;
using SwineBot.Senders;
using SwineBot.Actions.Commands.Duel;

namespace SwineBot.Updates;

public class InlineQueryHandler(ILogger<InlineQueryHandler> logger, UserContext context, IQueryResultsSender sender) : UpdateTypeHandler<Telegram.Bot.Types.InlineQuery>
{
    public override async Task<UpdateHandleResult> Handle(InlineQuery query, CancellationToken token)
    {
        if (query.ChatType is ChatType.Channel or ChatType.Private)
            return UpdateHandleResult.InlineQueryWrongChatType; 

        logger.LogInformation("Received inline query with text: \"{text}\" from user [{id}]", query.Query, query.From.Id);

        var isPrivate = query.ChatType is ChatType.Sender;

        var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == query.From.Id);
        if (user is null)
            return UpdateHandleResult.InlineQueryUserNotFound;

        if (!long.TryParse(query.Query, out var tgGroupId))
            return UpdateHandleResult.InlineQueryGroupIdNotLong;

        var group = await context.Groups.FirstOrDefaultAsync(g => g.TelegramId == tgGroupId);
        if (group is null)
            return UpdateHandleResult.InlineQueryGroupNotFound;

        var swine = await context.Swines
            .Where(s => s.GroupId == group.GroupId)
            .FirstOrDefaultAsync(s => s.OwnerId == user.UserId);

        if (swine is null)
            return UpdateHandleResult.InlineQuerySwineNotFound;

        IEnumerable<IQueryResult> results;

        try
        {
            var opps = await context.GetOpponents(user.UserId, group.GroupId);

            results = opps
                .Select(x => new DuelOpponent(x.Owner.TelegramId, x.Swine.Weight, x.Owner.FirstName, x.Swine.Name))
                .Cast<IQueryResult>();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to collect results, exception: \"{err}\"", e.ToString());
            return UpdateHandleResult.DatabaseFail;
        }

        try
        {
            await sender.Send(query.Id, results);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to answer inline query, exception: \"{err}\"", e.ToString());
            return UpdateHandleResult.SendMessageFail;
        }

        return UpdateHandleResult.InlineQueryOK;
    }
}
