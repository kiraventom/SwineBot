using Serilog;
using SwineBot.Model;
using SwineBot.Text;
using Telegram.Bot.Types;

namespace SwineBot.BotMessages;

public abstract class BotMessage(ILogger logger)
{
    private bool _isInited;

    protected ILogger Logger { get; } = logger;

    public MessageText Text { get; } = new();

    public string PhotoFilePath { get; protected set; }

    public async Task Init(UserContext userContext, ChatId chatId, int userId)
    {
        if (_isInited)
            return;

        var group = userContext.Groups.First(g => g.TelegramId == chatId.Identifier);
        var swine = group.Swines.First(s => s.OwnerId == userId);
        await InitInternal(userContext, swine.SwineId);

        _isInited = true;
    }
    
    protected abstract Task InitInternal(UserContext userContext, int swineId);
}
