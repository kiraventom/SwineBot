namespace SwineBot.BotMessages.Start.Actions.Params;

public abstract class StartLinkParam
{
    public const char SEPARATOR = '=';

    public abstract string Name { get; }
    public abstract string Value { get; }

    public override string ToString() => $"{Name}{SEPARATOR}{Value}";
}

