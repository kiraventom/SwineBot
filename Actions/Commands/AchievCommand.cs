using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class AchievCommand(ILogger<AchievCommand> logger, IMessageFactory messageFactory) : Command<AchievementsMessage>(logger, messageFactory)
{
    public override string Name => "/achiev";
    public override string Description => "Посмотреть достижения своего свина \U0001F3C6";
}

