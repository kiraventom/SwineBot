namespace SwineBot.Actions.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class CommandInfoAttribute(string handle, string description) : Attribute
{
    public string Handle { get; } = handle;
    public string Description { get; } = description;
    public string Title { get; init; } = handle;
    public bool Hidden { get; init; } 
}
