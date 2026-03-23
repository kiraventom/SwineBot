using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Model;
using SwineBot.Text;
using Telegram.Bot.Types;

namespace SwineBot.BotMessages;

public interface IMessageFactory
{
    T Create<T>(object[] args = null) where T : BotMessage;
}

public class MessageFactory(IServiceProvider sp) : IMessageFactory
{
    public T Create<T>(object[] args = null) where T : BotMessage => ActivatorUtilities.CreateInstance<T>(sp, args);
}

public abstract class BotMessage(ILogger<BotMessage> Logger)
{
    private bool _isInited;

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
