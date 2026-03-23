using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class PrivateMessage(ILogger<PrivateMessage> Logger) : BotMessage(Logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        Text.Bold("Для использования бота его необходимо добавить в группу");
        return Task.CompletedTask;
    }
}

