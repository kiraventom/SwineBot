namespace SwineBot.Actions.Commands;

public record CommandInfo(string Handle, string Title, string Description, bool Hidden, bool Callable, Type CommandType);

