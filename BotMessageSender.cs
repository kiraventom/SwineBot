using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public class BotMessageSender(ILogger logger, ITelegramBotClient client)
{
    public async Task<Message> Send(UserContext userContext, int userId, BotMessage botMessage)
    {
        var userModel = userContext.Users.First(u => u.UserId == userId);

        Message message;

        try
        {
            await botMessage.Init(userContext, userModel);
        }
        catch (Exception e)
        {
            if (botMessage is InvalidMessage)
            {
                logger.Fatal("Failed to initialize {invalidMessageName}, shit got real", nameof(InvalidMessage));
                return null;
            }
            else
            {
                logger.Fatal(e.ToString());

                var invalidMessage = new InvalidMessage(logger);
                var sentMessage = await Send(userContext, userId, invalidMessage);
                return sentMessage;
            }
        }

        try
        {
            var text = botMessage.Text.ToString();

            message = await client.SendMessage(chatId: userModel.TelegramId, text: text, parseMode: ParseMode.MarkdownV2);

            logger.Information("Sent '{text}' to [{id}], messageId [{messageId}]", text, userModel.UserId, message.MessageId);
        }
        catch (Exception e)
        {
            logger.Fatal(e.ToString());
            return null;
        }

        return message;
    }
}
