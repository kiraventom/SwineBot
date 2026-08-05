namespace SwineBot.Actions.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class CommandInfoAttribute(string handle, string description) : Attribute
{
    public string Handle { get; } = handle;
    public string Description { get; } = description;
    public string Title { get; init; } = handle;

    public CommandType Type { get; init; } = CommandType.Normal;
    public bool Hidden => Type is CommandType.Hidden or CommandType.Service;
    public bool Uncallable => Type is CommandType.Service;
}
