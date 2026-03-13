using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class PrivateMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        Text.Bold("Для использования бота его необходимо добавить в группу");
        return Task.CompletedTask;
    }
}

