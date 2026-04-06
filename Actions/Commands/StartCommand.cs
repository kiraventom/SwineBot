using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Start;
using SwineBot.BotMessages.Start.Actions;
using SwineBot.Model;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

[CommandInfo("/start", "Вывести это сообщение \U0001F928")]
public class StartCommand(ILogger<StartCommand> logger, UserContext context, IMessageFactory messageFactory, IStartLinkParser parser, ICommandInfos commandInfos) : ICommand
{
    public async Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            return [ messageFactory.Create<StartMessage, StartViewModel>(new StartViewModel(commandInfos.Infos)) ];
        }

        var didParse = parser.TryParse(parameter, out var action);

        if (!didParse)
        {
            logger.LogWarning("Failed to parse {0} as start link action", parameter);
            return [ messageFactory.Create<StartMessage, StartViewModel>(new StartViewModel(commandInfos.Infos)) ];
        }

        return [ await BuildSetSwineMessage(update, action) ];
    }

    private async Task<IBotMessage> BuildSetSwineMessage(Update update, IStartLinkAction action)
    {
        var setSwineViewModel = new SetPrivateSwineViewModel();
        action.Execute(setSwineViewModel);

        if (!update.IsPrivateChat)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{setSwineViewModel.SwineId}]: message not private");

        var swineToSet = await context.Swines.FirstOrDefaultAsync(s => s.SwineId == setSwineViewModel.SwineId);
        if (swineToSet is null)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{setSwineViewModel.SwineId}]: no such swine in db");

        if (swineToSet.OwnerId != update.UserId)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{setSwineViewModel.SwineId}]: it belongs to [{swineToSet.OwnerId}]");

        (await context.Users.AsTracking().FirstAsync(u => u.UserId == update.UserId)).PrivateSwineId = setSwineViewModel.SwineId;

        var groupTitle = (await context.Groups.FirstAsync(g => g.GroupId == swineToSet.GroupId)).Title;

        setSwineViewModel = setSwineViewModel with { SwineName = swineToSet.Name, GroupTitle = groupTitle };

        var setSwineMessage = messageFactory.Create<SetPrivateSwineMessage, SetPrivateSwineViewModel>(setSwineViewModel);
        return setSwineMessage;
    }
}
