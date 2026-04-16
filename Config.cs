using System.Text.Json;

namespace SwineBot;

public class Config(string token, string username, string connectionString, string achievDataFile = null)
{
    /// <summary>
    /// Telegram bot token. Received from <a href="https://t.me/BotFather">BotFather</a>
    /// </summary>
    public string Token { get; } = token;
    ///
    /// <summary>
    /// Telegram username, without @
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// SQlite connection string to DB
    /// </summary>
    public string ConnectionString { get; } = connectionString;

    /// <summary>
    /// Achievement data file path. Optional
    /// </summary>
    public string AchievDataFile { get; } = achievDataFile;

    public static Config Load(string filepath)
    {
        using var configFile = File.OpenRead(filepath);
        return JsonSerializer.Deserialize<Config>(configFile, Common.JsonOptions);
    }

    public void Save(string filepath)
    {
        using var configFile = File.Create(filepath);
        JsonSerializer.Serialize<Config>(configFile, this, Common.JsonOptions);
    }
}
