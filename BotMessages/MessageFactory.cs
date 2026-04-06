using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public interface IMessageFactory
{
    TMessage Create<TMessage, TViewModel>(TViewModel viewModel) 
        where TMessage : BotMessage<TViewModel>, new() 
        where TViewModel : ViewModel;
}

public class MessageFactory(ILoggerFactory loggerFactory) : IMessageFactory
{
    public TMessage Create<TMessage, TViewModel>(TViewModel viewModel)
        where TMessage : BotMessage<TViewModel>, new()
        where TViewModel : ViewModel
    {
        var message = new TMessage();
        message.Init(loggerFactory.CreateLogger<TMessage>(), viewModel);
        return message;
    }
}

