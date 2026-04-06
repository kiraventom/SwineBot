namespace SwineBot.BotMessages.Start.Actions;

public interface IStartLinkAction
{
    public string Name { get; }
    bool TryParse(string str);
    string Build();

    void Execute(IActionTarget message);
}

