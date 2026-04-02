using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public interface IMessageFactory
{
    T Create<T>(params object[] args) where T : IBotMessage;
}

public class MessageFactory(IServiceProvider sp) : IMessageFactory
{
    public T Create<T>(params object[] args) where T : IBotMessage => ActivatorUtilities.CreateInstance<T>(sp, args);
}

public abstract class BotMessage(ILogger<BotMessage> Logger) : IBotMessage
{
    private bool _isInited;

    public bool IsPrivate { get; private set; }

    public MessageText Text { get; } = new();

    public string PhotoFilePath { get; protected set; }

    public async Task Init(UserContext userContext, int swineId, bool isPrivate)
    {
        if (_isInited)
            return;

        IsPrivate = isPrivate;

        await InitInternal(userContext, swineId);

        _isInited = true;
    }

    protected abstract Task InitInternal(UserContext userContext, int swineId);
}
