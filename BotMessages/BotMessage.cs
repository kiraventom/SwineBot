using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public abstract class BotMessage(ILogger logger)
{
    private bool _isInited;

    protected ILogger Logger { get; } = logger;

    public MessageText Text { get; } = new();

    public string PhotoFilePath { get; protected set; }

    public async Task Init(UserContext userContext, int userId)
    {
        if (_isInited)
            return;

        await InitInternal(userContext, userId);

        _isInited = true;
    }
    
    protected abstract Task InitInternal(UserContext userContext, int userId);
}
