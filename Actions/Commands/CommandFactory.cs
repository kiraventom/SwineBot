using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SwineBot.Actions.Commands;

public interface ICommandFactory
{
    T Create<T>() where T : ICommand;
    ICommand Create(string handle);
}

public class CommandFactory(ILogger<CommandFactory> logger, ICommandInfos infos, Config config, IServiceProvider sp) : ICommandFactory
{
    public T Create<T>() where T : ICommand => sp.GetRequiredService<T>();

    public ICommand Create(string fullCommandText)
    {
        var callableInfos = infos.Infos.Where(i => i.Callable);
        foreach (var info in callableInfos)
        {
            if (CommandUtils.IsMatch(info.Handle, fullCommandText, config.Username))
                return (ICommand)sp.GetRequiredService(info.CommandType);
        }

        throw new NotSupportedException($"Failed to match command text \"{fullCommandText}\" against commands [ {string.Join(", ", callableInfos.Select(h => h.Handle))} ]");
    }
}
