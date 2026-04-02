using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.Model;

namespace SwineBot.BotMessages.Start;

public class StartMessage : BotMessage, IStaticMessage
{
    private static IReadOnlyCollection<ICommand> _commands;

    public StartMessage(ILogger<StartMessage> Logger, IEnumerable<ICommand> commands) : base(Logger)
    {
        if (_commands is not null)
            return;

        var commandsList = commands.ToList();

        commandsList.Sort((a, b) => a.Title.CompareTo(b.Title));

        var startIndex = commandsList.FindIndex(c => c is StartCommand);
        var startCommand = commandsList[startIndex];
        commandsList.RemoveAt(startIndex);
        commandsList.Insert(0, startCommand);

        _commands = commandsList;
    }

    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        Text.Bold("\U0001F437 Бот с кормлением свинок \U0001F43D").LineBreak()
            .LineBreak()
            .Italic("Доступные команды:").LineBreak();

        foreach (var command in _commands)
        {
            Text.Verbatim(command.Title).Verbatim(" - ").Verbatim(command.Description).LineBreak();
        }

        return Task.CompletedTask;
    }
}
