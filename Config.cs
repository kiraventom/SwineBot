using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwineBot;

[method: JsonConstructor]
public class Config(string token, string username, string userConnectionString)
{
    public static Config Instance { get; private set; }

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
        if (Instance is not null)
            return Instance;

        try
        {
            using var configFile = File.OpenRead(filepath);
            Instance = JsonSerializer.Deserialize<Config>(configFile, CommonOptions.Json);
        }
        catch (Exception)
        {
        }

        return Instance;
    }
}
