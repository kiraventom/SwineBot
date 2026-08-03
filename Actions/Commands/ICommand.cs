using SwineBot.BotMessages;
using SwineBot.Updates;

namespace SwineBot.Actions.Commands;

public interface ICommand
{
    Task<IReadOnlyCollection<IBotMessage>> Execute(Update update, string parameter = null);
}

