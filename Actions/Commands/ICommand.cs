using SwineBot.BotMessages;
using Telegram.Bot.Types;

namespace SwineBot.Actions.Commands;

public interface ICommand
{
    Task<IReadOnlyCollection<IBotMessage>> Execute(Updates.Update update, string parameter = null);
    Task AfterMessageSend(Updates.Update update, IBotMessage message, Message sentMessage);
}

