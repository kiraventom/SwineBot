using System.Reflection;
using Microsoft.Extensions.Logging;

namespace SwineBot.Actions.Commands;

public interface ICommandInfos
{
    IReadOnlyList<CommandInfo> Infos { get; }
}

public class CommandInfos : ICommandInfos
{
    public IReadOnlyList<CommandInfo> Infos { get; }

    public CommandInfos(ILogger<CommandInfos> logger)
    {
        var list = new List<CommandInfo>();

        var baseType = typeof(ICommand);
        var types = Assembly.GetAssembly(baseType).GetTypes().Where(t => t.IsAssignableTo(baseType) && t.IsClass && !t.IsAbstract);
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<CommandInfoAttribute>();
            if (attribute is null)
            {
                logger.LogWarning("Type \"{typename}\" inherits \"{basename}\", is class and is not abstract, but does not have [{attrtype}]", type.Name, baseType.Name, nameof(CommandInfoAttribute));
                continue;
            }

            if (string.IsNullOrWhiteSpace(attribute.Handle))
            {
                logger.LogWarning("Type \"{typename}\" inherits \"{basename}\", is class and is not abstract, but [{attrtype}].{handle} is null or empty", type.Name, baseType.Name, nameof(CommandInfoAttribute), nameof(CommandInfoAttribute.Handle));
                continue;
            }

            if (string.IsNullOrWhiteSpace(attribute.Title))
            {
                logger.LogWarning("Type \"{typename}\" inherits \"{basename}\", is class and is not abstract, but [{attrtype}].{title} is null or empty", type.Name, baseType.Name, nameof(CommandInfoAttribute), nameof(CommandInfoAttribute.Title));
                continue;
            }

            if (string.IsNullOrWhiteSpace(attribute.Description))
            {
                logger.LogWarning("Type \"{typename}\" inherits \"{basename}\", is class and is not abstract, but [{attrtype}].{desc} is null or empty", type.Name, baseType.Name, nameof(CommandInfoAttribute), nameof(CommandInfoAttribute.Description));
                continue;
            }

            var commandInfo = new CommandInfo(attribute.Handle, attribute.Title, attribute.Description, type);
            list.Add(commandInfo);
        }

        Infos = list;
    }
}
