using Microsoft.Extensions.Logging;

namespace SwineBot.BotMessages;

public class InvalidMessage(ILogger<InvalidMessage> logger) : BotMessage(logger), IStaticMessage
{
    protected override Task InitInternal(Update update)
    {
        Text.Bold("Что-то пошло не так :(");
        return Task.CompletedTask;
    }
}
