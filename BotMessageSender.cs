using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SwineBot;

public interface IBotMessageSender
{
    event BeforeMessageSendDelegate BeforeMessageSend;
    Task<Message> Send(UserContext context, ChatId chatId, int userId, BotMessage botMessage);
}

public class BotMessageSender(ILogger<BotMessageSender> Logger, ITelegramBotClient Client, IMessageFactory MessageFactory) : IBotMessageSender
{
    public event BeforeMessageSendDelegate BeforeMessageSend;

    private async Task<bool> InitMessage(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage)
    {
        try
        {
            await botMessage.Init(userContext, chatId, userId);
            return true;
        }
        catch (Exception e)
        {
            if (botMessage is InvalidMessage)
            {
                Logger.LogCritical("Failed to initialize {invalidMessageName}, shit got real", nameof(InvalidMessage));
                throw;
            }
            else
            {
                Logger.LogCritical(e.ToString());
                return false;
            }
        }
    }

    public async Task<Message> Send(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage)
    {
        try
        {
            var didInit = await InitMessage(userContext, chatId, userId, botMessage);
            if (didInit == false)
            {
                return await Send(userContext, chatId, userId, MessageFactory.Create<InvalidMessage>());
            }
        }
        catch
        {
            return null;
        }

        if (BeforeMessageSend != null)
            await BeforeMessageSend.Invoke(userContext, chatId, userId, botMessage);

        try
        {
            Message message;

            var text = botMessage.Text.ToString();

            if (botMessage.PhotoFilePath is null)
            {
                message = await Client.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.MarkdownV2, linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });

            }
            else
            {
                using (var stream = File.OpenRead(botMessage.PhotoFilePath))
                {
                    var photo = InputFile.FromStream(stream);
                    message = await Client.SendPhoto(chatId: chatId, photo: photo, caption: text, parseMode: ParseMode.MarkdownV2);
                }

                File.Delete(botMessage.PhotoFilePath);
            }

            Logger.LogInformation("Sent '{text}' to [{id}], messageId [{messageId}]", text, chatId, message.MessageId);

            return message;
        }
        catch (Exception e)
        {
            Logger.LogCritical(e.ToString());
            return null;
        }
    }
}

public delegate Task BeforeMessageSendDelegate(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage);
