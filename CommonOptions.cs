using System.Globalization;
using System.Text.Json;

namespace SwineBot;

public static class Common
{
    public static JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true, AllowTrailingCommas = true
    };

    public static CultureInfo RuCulture { get; } = CultureInfo.GetCultureInfo("ru");
}

