namespace SwineBot.BotMessages;

public interface IBotMessage
{
    string Text { get; }
    byte[] PhotoBytes { get; }
}


