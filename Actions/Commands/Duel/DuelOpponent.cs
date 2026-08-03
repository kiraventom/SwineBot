using Telegram.Bot.Types.InlineQueryResults;
using SwineBot.Senders;

namespace SwineBot.Actions.Commands.Duel;

public record DuelOpponent(long TelegramId, int Weight, string Caption) : IQueryResult
{
    public InlineQueryResultContact ToContact() => new InlineQueryResultContact(TelegramId.ToString(), Weight.ToString(), Caption)
    {
        InputMessageContent = new InputTextMessageContent($"/duel {TelegramId}")
    };
}
