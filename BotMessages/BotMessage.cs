using Microsoft.Extensions.Logging;
using SwineBot.Keyboard;
using SwineBot.Text;
using SwineBot.ViewModels;
using Telegram.Bot.Types.ReplyMarkups;

namespace SwineBot.BotMessages;

public abstract class BotMessage<TViewModel> : IBotMessage where TViewModel : ViewModel
{
    string IBotMessage.Text => Text.ToString();
    InlineKeyboardMarkup IBotMessage.Keyboard => Keyboard.ToMarkup();

    public virtual bool Notify => true;

    public MessageText Text { get; } = new();
    public byte[] PhotoBytes { get; protected set; }

    protected BotKeyboard Keyboard { get; } = new();

    public abstract void Init<TMessage>(ILogger<TMessage> logger, TViewModel viewModel) where TMessage : BotMessage<TViewModel>;
}
