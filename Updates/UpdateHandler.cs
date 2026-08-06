using Microsoft.Extensions.Logging;

namespace SwineBot.Updates;

public class UpdateHandler(ILogger<UpdateHandler> logger, MessageHandler messageHandler, InlineQueryHandler inlineQueryHandler, CallbackQueryHandler callbackQueryHandler) : IUpdateHandler
{
    public Task<UpdateHandleResult> Handle(Telegram.Bot.Types.Update tgUpdate, CancellationToken token)
    {
        logger.LogInformation("Received update: {updateType}", tgUpdate.Type);

        if (tgUpdate.Message is { } tgMessage)
            return messageHandler.Handle(tgMessage, token);

        if (tgUpdate.InlineQuery is {} inlineQuery)
            return inlineQueryHandler.Handle(inlineQuery, token);

        if (tgUpdate.CallbackQuery is {} callbackQuery)
            return callbackQueryHandler.Handle(callbackQuery, token);

        return Task.FromResult(UpdateHandleResult.OtherUpdate);
    }
}
