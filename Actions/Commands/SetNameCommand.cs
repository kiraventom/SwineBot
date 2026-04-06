using Microsoft.EntityFrameworkCore;
using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo(COMMAND_NAME, "Поменять имя свинки \u270f", Title = $"{COMMAND_NAME} <имя>")]
public class SetNameCommand(UserContext context, IMessageFactory messageFactory, AchievementController achievController) : Command<NewNameMessage, SetNameViewModel>(messageFactory, achievController)
{
    private const int MAX_NAME_LENGTH = 50;
    public const string COMMAND_NAME = "/setname";

    protected override async Task<SetNameViewModel> ExecuteInternal(Update update, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return new SetNameViewModel(false, false, null);

        var name = parameter.Split('\n', StringSplitOptions.RemoveEmptyEntries).First().Trim();
        var swine = await context.Swines.AsTracking().FirstAsync(s => s.SwineId == update.SwineId);

        if (swine.Name == name)
            return new SetNameViewModel(true, true, name);

        if (name.Length > MAX_NAME_LENGTH)
            name = name.Substring(0, MAX_NAME_LENGTH);

        swine.Name = name;

        return new SetNameViewModel(true, false, name);
    }
}
