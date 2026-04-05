using Microsoft.Extensions.DependencyInjection;

namespace SwineBot.BotMessages;


public interface IMessageFactory
{
    T Create<T>(params object[] args) where T : IBotMessage;
}

public class MessageFactory(IServiceProvider sp) : IMessageFactory
{
    public T Create<T>(params object[] args) where T : IBotMessage => ActivatorUtilities.CreateInstance<T>(sp, args);
}

