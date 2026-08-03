namespace SwineBot.Updates;

public interface IUpdateHandler
{
    Task<UpdateHandleResult> Handle(Telegram.Bot.Types.Update update, CancellationToken token);
}

