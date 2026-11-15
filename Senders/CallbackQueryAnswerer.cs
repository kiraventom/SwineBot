using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace SwineBot.Senders;

public interface ICallbackQueryAnswerer
{
    Task Answer(string callbackQueryId, string text = null);
}

/// <summary>
/// Stops the progress indicator of a pressed inline button. <c>text</c> is shown as a
/// small toast above the chat, not as an alert dialog.
/// </summary>
public class CallbackQueryAnswerer(ILogger<CallbackQueryAnswerer> logger, ITelegramBotClient client) : ICallbackQueryAnswerer
{
    public async Task Answer(string callbackQueryId, string text = null)
    {
        try
        {
            await client.AnswerCallbackQuery(callbackQueryId, text: text);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to answer callback query [{id}]", callbackQueryId);
        }
    }
}
