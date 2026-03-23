using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger<InfoCommand> logger, IMessageFactory messageFactory) : Command<InfoMessage>(logger, messageFactory)
{
    public override string Name => "/info";
    public override string Description => "Получить инфу о своём свине \u2139\ufe0f";
}
