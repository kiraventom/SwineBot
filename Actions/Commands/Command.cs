using SwineBot.Achievements;
using SwineBot.BotMessages;
using SwineBot.ViewModels;

namespace SwineBot.Actions.Commands;

public abstract class Command<TMessage, TViewModel>(IMessageFactory messageFactory, AchievementController achievController) : ICommand 
    where TMessage : BotMessage<TViewModel>, new()
    where TViewModel : ViewModel
{
    protected AchievementController AchievController { get; } = achievController;

    protected abstract Task<TViewModel> ExecuteInternal(Update update, string parameter);

    public async Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter = null)
    {
        var viewModel = await ExecuteInternal(update, parameter);

        var achievMessages = await AchievController.GetAchievMessages(update.SwineId.Value, viewModel);
        var mainMessage = messageFactory.Create<TMessage, TViewModel>(viewModel);

        return achievMessages.OfType<IBotMessage>().Append(mainMessage).ToList();
    }
}

