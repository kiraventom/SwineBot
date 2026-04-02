using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SwineBot.BotMessages.Start.Actions;

namespace SwineBot.BotMessages.Start;

public class StartLinkBuilder(Config config)
{
    public const string START_LINK_HEAD = "t.me/";
    public const string START_LINK_TAIL = "?start=";

    public string Build(IStartLinkAction action)
    {
        var actionStr = action.Build();
        var utf8Bytes = Encoding.UTF8.GetBytes(actionStr);
        var encoded = WebEncoders.Base64UrlEncode(utf8Bytes);
        return $"{START_LINK_HEAD}{config.Username}{START_LINK_TAIL}{encoded}";
    }
}

