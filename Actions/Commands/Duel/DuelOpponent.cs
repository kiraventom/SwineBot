using Telegram.Bot.Types.InlineQueryResults;
using SwineBot.Senders;

namespace SwineBot.Actions.Commands.Duel;

public record DuelOpponent(long TelegramId, int Weight, string Name, string SwineName) : IQueryResult
{
    public InlineQueryResultContact ToContact() => new InlineQueryResultContact(TelegramId.ToString(), $"{Weight} кг", $"{SwineName} ({Name})")
    {
        InputMessageContent = new InputTextMessageContent($"/duel {TelegramId}")
    };
}
