using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class InvalidMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        Text.Bold("Что-то пошло не так :(");
        return Task.CompletedTask;
    }
}
