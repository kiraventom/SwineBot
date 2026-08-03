using SwineBot.BotMessages;

namespace SwineBot.Updates;

public record UpdateReply(Update Update, IReadOnlyCollection<IBotMessage> Messages);

