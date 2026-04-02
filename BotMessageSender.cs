using Microsoft.Extensions.Logging;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Start;
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

    private async Task<BotMessage> InitMessage(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage)
    {
        try
        {
            int? swineId = userContext.GetSwineId(chatId, userId);
            if (swineId is null)
            {
                swineId = -1;

                if (botMessage is not IStaticMessage and not PiggeryMessage)
                    botMessage = MessageFactory.Create<PiggeryMessage>(userId);
            }

            var isPrivate = userContext.IsPrivateChat(chatId);
            await botMessage.Init(userContext, swineId.Value, isPrivate);
            return botMessage;
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Failed to initialize {message}", botMessage.GetType().Name);

            if (botMessage is InvalidMessage)
            {
                Logger.LogCritical(e, "Failed to initialize {invalidMessageName}, shit got real", nameof(InvalidMessage));
                throw;
            }

            var invalidMessage = MessageFactory.Create<InvalidMessage>();
            return await InitMessage(userContext, chatId, userId, invalidMessage);
        }
    }

    public async Task<Message> Send(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage)
    {
        try
        {
            botMessage = await InitMessage(userContext, chatId, userId, botMessage);
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

            if (botMessage is IPinnableMessage { ShouldPin: true })
            {
                await Client.UnpinAllChatMessages(chatId);
                await Client.PinChatMessage(chatId, message.MessageId);

                Logger.LogInformation("Pinned [{messageId}] in chat [{id}]", message.MessageId, chatId);
            }

            return message;
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Sending message failed");
            return null;
        }
    }
}

// TODO: Rewrite the whole app and throw away passing UserContext in method parameters
// Change Singletons to Scoped when needed, pass actual data instead of Context when possible
// Minimize usage of IServiceScopeFactory
// Replace this event with direct call (connect via DI ctor)
public delegate Task BeforeMessageSendDelegate(UserContext userContext, ChatId chatId, int userId, BotMessage botMessage);
