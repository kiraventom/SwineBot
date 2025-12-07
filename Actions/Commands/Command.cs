using Serilog;

namespace SwineBot.Actions.Commands;

public abstract class Command(ILogger logger, BotMessageSender sender) : UserAction(logger, sender);
