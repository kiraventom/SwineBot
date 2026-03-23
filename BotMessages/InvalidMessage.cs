using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class InvalidMessage(ILogger<InvalidMessage> Logger) : BotMessage(Logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        Text.Bold("Что-то пошло не так :(");
        return Task.CompletedTask;
    }
}
