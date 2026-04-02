using SwineBot.BotMessages.Start.Actions.Params;

namespace SwineBot.BotMessages;

public class SwineIdStartLinkParam(int swineId) : StartLinkParam
{
    public const string NAME = "si";

    public override string Name => NAME;
    public override string Value => SwineId.ToString();

    public int SwineId { get; } = swineId;
}

