using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwineBot;

[method: JsonConstructor]
public class Config(string token, string username, string userConnectionString)
{
    /// <summary>
    /// Telegram bot token. Received from <a href="https://t.me/BotFather">BotFather</a>
    /// </summary>
    public string Token { get; } = token;
    ///
    /// <summary>
    /// Telegram username
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// SQlite connection string to User DB
    /// </summary>
    public string UserConnectionString { get; } = userConnectionString;

    public static Config Load(string filepath)
    {
        using var configFile = File.OpenRead(filepath);
        return JsonSerializer.Deserialize<Config>(configFile, Common.JsonOptions);
    }

    public void Save(string filepath)
    {
        using var configFile = File.OpenWrite(filepath);
        JsonSerializer.Serialize<Config>(configFile, this, Common.JsonOptions);
    }
}
