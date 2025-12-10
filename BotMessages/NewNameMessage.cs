using Microsoft.EntityFrameworkCore;
using Serilog;
using SwineBot.Actions.Commands;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.BotMessages;

public class NewNameMessage(ILogger logger, string name) : BotMessage(logger)
{
    protected override Task InitInternal(UserContext userContext, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Text.Italic("Формат команды:")
                .LineBreak()
                .Monospace($"{SetNameCommand.COMMAND_NAME} <новое имя>");

            return Task.CompletedTask;
        }

        name = name.Trim();
        
        var swine = userContext.Swines.First(s => s.OwnerId == userId);
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
