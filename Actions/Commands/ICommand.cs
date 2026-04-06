using SwineBot.BotMessages;

namespace SwineBot.Actions.Commands;

public interface ICommand
{
    Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter = null);
}

