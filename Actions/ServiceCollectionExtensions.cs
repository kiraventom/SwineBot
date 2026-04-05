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
        var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseActionType));

        foreach (var actionType in actionTypes)
            collection.AddTransient(typeof(IStartLinkAction), actionType);

        return collection;
    }

    public static IServiceCollection AddUserActions(this IServiceCollection collection)
    {
        var baseActionType = typeof(UserAction);
        var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseActionType));

        foreach (var actionType in actionTypes)
        {
            collection.AddTransient(actionType);
            collection.AddTransient(typeof(UserAction), sp => sp.GetRequiredService(actionType));

            if (actionType.IsAssignableTo(typeof(ICommand)))
                collection.AddTransient(typeof(ICommand), sp => sp.GetRequiredService(actionType));
        }

        return collection;
    }
}


