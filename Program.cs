using Serilog;
using Serilog.Core;
using Serilog.Events;
using SwineBot.Actions;
using SwineBot.Actions.Commands;
using Telegram.Bot;

namespace SwineBot;

internal class Program
{
    private const string PROJECT_NAME = "SwineBot";

    private static async Task Main(string[] args)
    {
        var projectDirPath = GetConfigDirPath();
        Directory.CreateDirectory(projectDirPath);

        if (!TryLoadConfig(projectDirPath, out var config))
        {
            Console.WriteLine("Couldn't parse config, exiting");
            return;
        }

        var logger = InitLogger(projectDirPath);

        logger.Information("===== ENTRY POINT =====");

        if (!TryInitTelegramClient(logger, config.Token, out var client))
        {
            logger.Fatal("Couldn't init {TelegramBotClient}, exiting", nameof(TelegramBotClient));
            return;
        }

        var sender = new BotMessageSender(logger, client);
        var commands = BuildCommands(logger, sender);

        var telegramController = new TelegramController(logger, commands.ToList());
        telegramController.StartReceiving(client);

        while (true)
        {
            if (Console.In.Peek() is (int)'q' or (int)'Q')
                return;

            await Task.Delay(10);
        }
    }

    private static string GetConfigDirPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataDirPath, PROJECT_NAME);
        }

        var homeDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDirPath, ".config", PROJECT_NAME);
    }

    private static Logger InitLogger(string projectDirPath)
    {
        var logsDirPath = Path.Combine(projectDirPath, "logs");
        Directory.CreateDirectory(logsDirPath);
        var logFilePath = Path.Combine(logsDirPath, $"{PROJECT_NAME}.log");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        Log.Logger = logger;

        return logger;
    }

    private static bool TryLoadConfig(string projectDirPath, out Config config)
    {
        var configFilePath = Path.Combine(projectDirPath, "config.json");
        config = Config.Load(configFilePath);
        return config is not null;
    }

    private static bool TryInitTelegramClient(ILogger logger, string token, out TelegramBotClient client)
    {
        try
        {
            client = new TelegramBotClient(token);
            return true;
        }
        catch (Exception e)
        {
            client = null;
            logger.Error(e.ToString());
            return false;
        }
    }

   private static IEnumerable<UserAction> BuildCommands(ILogger logger, BotMessageSender sender)
   {
      yield return new StartCommand(logger, sender);
      yield return new FeedCommand(logger, sender);
      yield return new InfoCommand(logger, sender);
      yield return new SetNameCommand(logger, sender);
   }
}
