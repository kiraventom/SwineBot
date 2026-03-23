using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SwineBot.Actions.Commands;

namespace SwineBot.Actions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserActions(this IServiceCollection collection)
    {
        var baseActionType = typeof(UserAction);
        var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseActionType));

        foreach (var actionType in actionTypes)
        {
            collection.AddSingleton(actionType);
            collection.AddSingleton(typeof(UserAction), sp => sp.GetRequiredService(actionType));

            if (actionType.IsAssignableTo(typeof(ICommand)))
                collection.AddSingleton(typeof(ICommand), sp => sp.GetRequiredService(actionType));
        }

        return collection;
    }
}


