using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace SwineBot.Senders;

public interface IQueryResult
{
    InlineQueryResultContact ToContact();
}

public interface IQueryResultsSender
{
    Task Send(string inlineQueryId, IEnumerable<IQueryResult> results);
}

public class QueryResultsSender(ILogger<QueryResultsSender> logger, ITelegramBotClient client) : IQueryResultsSender
{
    public async Task Send(string inlineQueryId, IEnumerable<IQueryResult> results)
    {
        var inlineQueryResults = results.Select(r => r.ToContact());
        await client.AnswerInlineQuery(inlineQueryId, inlineQueryResults, isPersonal: true, cacheTime: 1);
    }
}

