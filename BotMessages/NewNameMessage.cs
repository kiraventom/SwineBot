using Microsoft.Extensions.Logging;
using SwineBot.Actions.Commands;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class NewNameMessage(ILogger<NewNameMessage> logger, UserContext context, string name) : BotMessage(logger)
{
    protected override async Task InitInternal(Update update)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Text.Italic("Формат команды:")
                .LineBreak()
                .Monospace($"{SetNameCommand.COMMAND_NAME} <новое имя>");

            return;
        }

        name = name.Split('\n', StringSplitOptions.RemoveEmptyEntries).First().Trim();

        var swine = context.Swines.First(s => s.SwineId == update.SwineId);
        if (swine.Name == name)
        {
            Text.Italic("Свина и так зовут \"").Bold(name).Italic("\" \U0001F914");
            return;
        }

        swine.Name = name;
        await context.SaveChangesAsync();

        Text.Bold(swine.Name).Verbatim(" радостно хрюкает, будто подпевая своему новому имени!");
    }
}
