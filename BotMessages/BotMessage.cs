using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public abstract class BotMessage(ILogger logger)
{
    private bool _isInited;

    protected ILogger Logger { get; } = logger;

    public MessageText Text { get; } = new();

    public async Task Init(UserContext userContext, User user)
    {
        if (_isInited)
            return;

        await InitInternal(userContext, user.UserId);

        _isInited = true;
    }
    
    protected abstract Task InitInternal(UserContext userContext, int userId);
}
