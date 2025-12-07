using Serilog;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class StartMessage(ILogger logger) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        Text.Bold("\U0001F437 Бот с кормлением свинок \U0001F43D").LineBreak()
            .LineBreak()
            .Italic("Доступные команды:").LineBreak()
            .Verbatim("/start — вывести это сообщение \U0001F928").LineBreak()
            .Verbatim("/feed — покормить своего свина \U0001F416").LineBreak()
            .Verbatim("/info — получить инфу о своём свине \u2139\ufe0f").LineBreak();

        return Task.CompletedTask;
    }
}
