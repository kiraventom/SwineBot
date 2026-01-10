using Serilog;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public class BotMessageSender(ILogger logger, ITelegramBotClient client)
{
    public event BeforeMessageSendDelegate BeforeMessageSend;

    private async Task<bool> InitMessage(UserContext userContext, int userId, BotMessage botMessage)
    {
        try
        {
            await botMessage.Init(userContext, userId);
            return true;
        }
        catch (Exception e)
        {
            if (botMessage is InvalidMessage)
            {
                logger.Fatal("Failed to initialize {invalidMessageName}, shit got real", nameof(InvalidMessage));
                throw;
            }
            else
            {
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }

    public async Task<Message> Send(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage)
    {
        try
        {
            var didInit = await InitMessage(userContext, userId, botMessage);
            if (didInit == false)
            {
                return await Send(userContext, chatId, userId, new InvalidMessage(logger));
            }
        }
        catch
        {
            return null;
        }

        await BeforeMessageSend?.Invoke(userContext, chatId, userId, botMessage);

        try
        {
            Message message;

            var text = botMessage.Text.ToString();

            if (botMessage.PhotoFilePath is null)
            {
                message = await client.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.MarkdownV2, linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });

            }
            else
            {
                using (var stream = File.OpenRead(botMessage.PhotoFilePath))
                {
                    var photo = InputFile.FromStream(stream);
                    message = await client.SendPhoto(chatId: chatId, photo: photo, caption: text, parseMode: ParseMode.MarkdownV2);
                }

                File.Delete(botMessage.PhotoFilePath);
            }

            logger.Information("Sent '{text}' to [{id}], messageId [{messageId}]", text, chatId, message.MessageId);

            return message;
        }
        catch (Exception e)
        {
            logger.Fatal(e.ToString());
            return null;
        }
    }
}

public delegate Task BeforeMessageSendDelegate(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage);

