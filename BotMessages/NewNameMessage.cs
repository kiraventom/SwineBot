using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class NewNameMessage(ILogger<NewNameMessage> Logger, string name) : BotMessage(Logger)
{
    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Text.Italic("Формат команды:")
                .LineBreak()
                .Monospace($"{SetNameCommand.COMMAND_NAME} <новое имя>");

            return Task.CompletedTask;
        }

        name = name.Trim();

        var swine = userContext.Swines.First(s => s.SwineId == swineId);
        if (swine.Name == name)
        {
            Text.Italic("Свина и так зовут \"").Bold(name).Italic("\" \U0001F914");
            return Task.CompletedTask;
        }

        swine.Name = name;

        Text.Bold(swine.Name).Verbatim(" радостно хрюкает, будто подпевая своему новому имени!");

        return Task.CompletedTask;
    }
}
