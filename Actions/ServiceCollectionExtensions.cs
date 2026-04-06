using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages.Start.Actions;

namespace SwineBot.Actions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStartLinkActions(this IServiceCollection collection)
    {
        var baseActionType = typeof(IStartLinkAction);
        var actionTypes = Assembly.GetAssembly(baseActionType).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseActionType));

        foreach (var actionType in actionTypes)
            collection.AddTransient(typeof(IStartLinkAction), actionType);

        return collection;
    }

    public static IServiceCollection AddCommands(this IServiceCollection collection)
    {
        var baseCommandType = typeof(ICommand);
        var commandTypes = Assembly.GetAssembly(baseCommandType).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseCommandType));

        foreach (var commandType in commandTypes)
            collection.AddTransient(commandType);

        return collection;
    }
}
