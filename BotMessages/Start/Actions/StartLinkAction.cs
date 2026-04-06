using System.Text;
using SwineBot.BotMessages.Start.Actions.Params;

namespace SwineBot.BotMessages.Start.Actions;

public abstract class StartLinkAction<T> : IStartLinkAction where T : IActionTarget
{
    public const char ACTION_PARAM_SEPARATOR = ':';
    public const char PARAMS_SEPARATOR = ';';

    public abstract string Name { get; }

    protected abstract IEnumerable<StartLinkParam> Params { get; }

    public bool TryParse(string str)
    {
        var actionParamSeparatorIndex = str.IndexOf(ACTION_PARAM_SEPARATOR);
        if (actionParamSeparatorIndex < 0)
            return false;

        var actionStr = str.Substring(0, actionParamSeparatorIndex);
        if (actionStr != Name)
            return false;

        var paramsStr = str.Substring(actionParamSeparatorIndex + 1);
        var split = paramsStr.Split(PARAMS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return TryParseParams(split);
    }

    protected abstract bool TryParseParams(IReadOnlyList<string> paramsArr);

    public string Build()
    {
        var sb = new StringBuilder();
        sb.Append(Name).Append(ACTION_PARAM_SEPARATOR);
        foreach (var p in Params)
        {
            sb.Append(p.ToString()).Append(PARAMS_SEPARATOR);
        }

        return sb.ToString();
    }

    public void Execute(IActionTarget message) => ExecuteInternal((T)message);

    protected abstract void ExecuteInternal(T message);
}

