namespace SwineBot.Updates;

public record Update(string Text, int? GroupId, int UserId, int? SwineId, long TelegramChatId, bool IsPrivateChat)
{
    public Recepient Recepient { get; } = new Recepient(TelegramChatId, IsPrivateChat);
}

