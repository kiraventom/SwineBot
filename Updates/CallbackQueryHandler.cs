using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SwineBot.BotMessages.Duels;
using SwineBot.Model;
using Telegram.Bot.Types;

namespace SwineBot.Updates;

public class CallbackQueryHandler(ILogger<CallbackQueryHandler> logger, UserContext context) : UpdateTypeHandler<Telegram.Bot.Types.CallbackQuery>
{
    public override async Task<UpdateHandleResult> Handle(CallbackQuery query, CancellationToken token)
    {
        var data = query.Data;
        string prefix;
        
        if (data.StartsWith(DuelRequestMessage.AcceptCallbackDataPrefix))
            prefix = DuelRequestMessage.AcceptCallbackDataPrefix;
        else if (data.StartsWith(DuelRequestMessage.DeclineCallbackDataPrefix))
            prefix = DuelRequestMessage.DeclineCallbackDataPrefix;
        else
            return UpdateHandleResult.CallbackQueryUnknownData;

        var duelRequestIdStr = data.Substring(prefix.Length);
        if (!int.TryParse(duelRequestIdStr, out var duelRequestId))
            return UpdateHandleResult.CallbackQueryInvalidDuelRequestId;

        var duelRequest = await context.DuelRequests.FirstOrDefaultAsync(dr => dr.RequestId == duelRequestId);
        if (duelRequest is null)
            return UpdateHandleResult.CallbackQueryDuelRequestNotFound;

        // TODO 
        // DuelRequest.Process(accept/decline);
        //
        return UpdateHandleResult.CallbackQueryOK;
    }
}

