using Serilog.Events;
using SwineBot.Achievements;
using SwineBot.Actions;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwineBot.Model;
using Microsoft.EntityFrameworkCore;
using static System.Environment;
using Serilog;
using SwineBot.BotMessages;
using SwineBot.Achievements.Checkers;
using SwineBot.BotMessages.Feed;
using SwineBot.BotMessages.Start;

namespace SwineBot;

public interface IDateTimeNowProvider
{
    DateTime UtcNow { get; }
}

public class DateTimeNowProvider : IDateTimeNowProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public record Paths(string ConfigDir, string DataDir);

internal class Program
{
    private const string PROJECT_NAME = "SwineBot";

    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Building host...");

        try
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Services
                // Singleton
                .AddSingleton<Paths>(BuildPaths)
                .AddSingleton<Config>(BuildConfig)
                .AddSerilog(ConfigureLogger)
                .AddSingleton<TelegramBotClientOptions>(BuildTelegramBotClientOptions)
                .AddSingleton<IDateTimeNowProvider, DateTimeNowProvider>()
                .AddSingleton<StartLinkBuilder>()
                .AddSingleton<ITelegramBotClient, TelegramBotClient>()
                .AddSingleton<IAchievementCheckerBuilders, AchievementCheckerBuilders>()

                // Transient
                .AddStartLinkActions()
                .AddTransient<IStartLinkParser, StartLinkParser>()
                .AddUserActions()
                .AddTransient<AchievementCheckerBuilder>()

                // Scoped
                .AddDbContext<UserContext>(ConfigureContext)
                .AddScoped<AchievementCheckerFactory>()
                .AddScoped<AchievementController>()
                .AddScoped<IFeedGeneratorFactory, FeedGeneratorFactory>()
                .AddScoped<IThrowupCalculatorFactory, ThrowupCalculatorFactory>()
                .AddScoped<IMessageFactory, MessageFactory>()
                .AddScoped<UserContextHelpers>()
                .AddScoped<IBotMessageSender, BotMessageSender>()
                .AddScoped<IUpdateHandler, UpdateHandler>()

                // Host
                .AddHostedService<AppService>();

            var host = builder.Build();

            Log.Information("Starting host...");

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to run host, terminating");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static TelegramBotClientOptions BuildTelegramBotClientOptions(IServiceProvider provider) => new TelegramBotClientOptions(provider.GetRequiredService<Config>().Token);

    private static void ConfigureContext(IServiceProvider provider, DbContextOptionsBuilder builder)
    {
        var config = provider.GetRequiredService<Config>();
        builder.UseSqlite(config.UserConnectionString);
    }

    private static Config BuildConfig(IServiceProvider provider)
    {
        var paths = provider.GetRequiredService<Paths>();
        var configFilePath = Path.Combine(paths.ConfigDir, "config.json");

        if (!File.Exists(configFilePath))
        {
            var defaultConfig = new Config("TOKEN", "@USERNAME_BOT", "Data Source=PATH_TO.db");
            defaultConfig.Save(configFilePath); 
            throw new InvalidOperationException($"Default config created at {configFilePath}. Fill it out and restart.");
        }

        return Config.Load(configFilePath);
    }

    private static Paths BuildPaths(IServiceProvider provider)
    {
        var appData = Environment.GetFolderPath(SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(SpecialFolder.LocalApplicationData);

        var configDirPath = Path.Combine(appData, PROJECT_NAME);
        var dataDirPath = Path.Combine(localAppData, PROJECT_NAME);

        Directory.CreateDirectory(configDirPath);
        Directory.CreateDirectory(dataDirPath);

        return new Paths(configDirPath, dataDirPath);
    }

    private static void ConfigureLogger(IServiceProvider provider, LoggerConfiguration logger)
    {
        var paths = provider.GetRequiredService<Paths>();
        var logsDirPath = Path.Combine(paths.DataDir, "logs");
        Directory.CreateDirectory(logsDirPath);
        var logFilePath = Path.Combine(logsDirPath, $"{PROJECT_NAME}.log");

        logger.MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);
    }
}

