using Microsoft.Extensions.Logging;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public abstract class BotMessage(ILogger<BotMessage> logger) : IBotMessage
{
    private bool _isInited;

    public MessageText Text { get; } = new();

    public string PhotoFilePath { get; protected set; }

    public async Task Init(Update update)
    {
        if (_isInited)
        {
            logger.LogWarning("Attempt to init {type} twice, update [ {update} ]", this.GetType().Name, update.ToString());
            return;
        }

        await InitInternal(update);

        _isInited = true;
    }

    protected abstract Task InitInternal(Update update);
}
