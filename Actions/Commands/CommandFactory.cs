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
        foreach (var info in infos.Infos)
        {
            if (CommandUtils.IsMatch(info.Handle, fullCommandText, config.Username))
                return (ICommand)sp.GetRequiredService(info.CommandType);
        }

        logger.LogWarning($"Failed to match command text \"{fullCommandText}\" against commands [ {string.Join(", ", infos.Infos.Select(h => h.Handle))} ]");
        return null;
    }
}
