using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;
using Update = SwineBot.Updates.Update;

namespace SwineBot.Senders;

public interface IQueryResult
{
    InlineQueryResultContact ToContact();
}

public interface IQueryResultsSender
{
    Task Send(Update update, IEnumerable<IQueryResult> results);
}

public class QueryResultsSender(ILogger<QueryResultsSender> logger, ITelegramBotClient client) : IQueryResultsSender
{
    public async Task Send(Update update, IEnumerable<IQueryResult> results)
    {
        var inlineQueryResults = results.Select(r => r.ToContact());
        await client.AnswerInlineQuery(update.InlineQueryId, inlineQueryResults, isPersonal: true);
    }
}

