namespace SwineBot.Updates;

public record Update(string Text, int? GroupId, int UserId, int? SwineId, long TelegramChatId, bool IsPrivateChat, string InlineQueryId = null);

