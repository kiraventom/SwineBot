using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public class InfoCommand(ILogger<InfoCommand> logger, IMessageFactory messageFactory) : Command<InfoMessage>(logger, messageFactory)
{
    public const string NAME = "/info";

    public override string Name => NAME;
    public override string Description => "Получить инфу о своём свине \u2139\ufe0f";
}
