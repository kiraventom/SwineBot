using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using IUpdateHandler = SwineBot.Updates.IUpdateHandler;

namespace SwineBot;

public class AppService(ILogger<AppService> Logger, IServiceScopeFactory spf, ITelegramBotClient Client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery]
        };

        Client.StartReceiving(OnUpdate, OnError, receiverOptions, cancellationToken: stoppingToken);
        Logger.LogInformation("Started listening");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnUpdate(ITelegramBotClient client, Telegram.Bot.Types.Update update, CancellationToken ct)
    {
        using var scope = spf.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
        var result = await updateHandler.Handle(update, ct);
        if (result is not Updates.UpdateHandleResult.MessageOK and not Updates.UpdateHandleResult.InlineQueryOK and not Updates.UpdateHandleResult.MessageSuccesfulMigration)
        {
            Logger.LogWarning("Update handle result not OK: {result}", result.ToString());
        }
    }

    private Task OnError(ITelegramBotClient client, Exception exception, CancellationToken ct)
    {
        Logger.LogError(exception, "Polling error");
        return Task.CompletedTask;
    }
}

