using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using SwineBot.Achievements;

namespace SwineBot;

public class AppService(ILogger<AppService> Logger, IServiceScopeFactory spf, ITelegramBotClient Client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message]
        };

        using var scope = spf.CreateScope();
        var messageSender = scope.ServiceProvider.GetRequiredService<IBotMessageSender>();
        var achievController = scope.ServiceProvider.GetRequiredService<IAchievementController>();
        messageSender.BeforeMessageSend += achievController.OnBeforeMessageSend;

        Client.StartReceiving(OnUpdate, OnError, receiverOptions, cancellationToken: stoppingToken);
        Logger.LogInformation("Started listening");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        using var scope = spf.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
        await updateHandler.Handle(update, ct);
    }

    private Task OnError(ITelegramBotClient client, Exception exception, CancellationToken ct)
    {
        Logger.LogError(exception, "Polling error");
        return Task.CompletedTask;
    }
}

