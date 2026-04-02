using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SwineBot.BotMessages.Start.Actions;

namespace SwineBot.BotMessages.Start;

public interface IStartLinkParser
{
    bool TryParse(string startLink, out IStartLinkAction action);
}

public class StartLinkParser(ILogger<StartLinkParser> logger, IEnumerable<IStartLinkAction> actions) : IStartLinkParser
{
    public bool TryParse(string encoded, out IStartLinkAction action)
    {
        action = null;

        byte[] bytes;
        try
        {
            bytes = WebEncoders.Base64UrlDecode(encoded);
        }
        catch (FormatException ex)
        {
            logger.LogWarning("Failed to parse encoded start link part {0}: {1}", (object)encoded, ex.ToString());
            return false;
        }

        string actionStr;

        try
        {
            actionStr = Encoding.UTF8.GetString(bytes);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Failed to convert bytes [ {0} ] to UTF8 string: {1}", string.Join(string.Empty, bytes.Select(b => b.ToString("X4"))), ex.ToString());
            return false;
        }

        foreach (var a in actions)
        {
            var didParse = a.TryParse(actionStr);
            if (didParse)
            {
                action = a;
                return true;
            }
        }

        logger.LogWarning("None of the actions [ {actions} ] parsed action string {str}", string.Join(", ", actions.Select(s => s.GetType().Name)), actionStr);
        return false;
    }
}

