using SwineBot.BotMessages.Start.Actions.Params;

namespace SwineBot.BotMessages.Start.Actions;

public class SetSwineAction : StartLinkAction<ISettableSwineId>
{
    public override string Name => "ss";

    public int SwineId { get; set; }

    protected override IEnumerable<StartLinkParam> Params => [ new SwineIdStartLinkParam(SwineId) ];

    protected override void ExecuteInternal(ISettableSwineId settable) => settable.SetSwineId(SwineId);

    protected override bool TryParseParams(IReadOnlyList<string> paramsArr)
    {
        if (paramsArr.Count != 1)
            return false;

        var param = paramsArr[0];
        var paramSpl = param.Split(StartLinkParam.SEPARATOR, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (paramSpl.Length != 2)
            return false;

        if (paramSpl[0] != SwineIdStartLinkParam.NAME)
            return false;

        if (!int.TryParse(paramSpl[1], out var swineId))
            return false;

        SwineId = swineId;
        return true;
    }
}

