using Microsoft.Extensions.Logging;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public abstract class BotMessage<TViewModel> : IBotMessage where TViewModel : ViewModel
{
    string IBotMessage.Text => Text.ToString();

    public MessageText Text { get; } = new();
    public byte[] PhotoBytes { get; protected set; }

    public abstract void Init<TMessage>(ILogger<TMessage> logger, TViewModel viewModel) where TMessage : BotMessage<TViewModel>;
}
