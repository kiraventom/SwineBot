using SwineBot.Model;

namespace SwineBot.Updates;

public record Recepient
{
    public long ChatId { get; }
    public bool IsPrivateChat { get; }

    public Recepient(long chatId, bool isPrivateChat)
    {
        ChatId = chatId;
        IsPrivateChat = isPrivateChat;
    }

    public static Recepient Group(UserContext context, int groupId)
    {
        var chatId = context.Groups.First(g => g.GroupId == groupId).TelegramId;
        return new Recepient(chatId, false);
    }

    public static Recepient User(UserContext context, int userId)
    {
        var chatId = context.Users.First(g => g.UserId == userId).TelegramId;        
        return new Recepient(chatId, true);
    }
}


