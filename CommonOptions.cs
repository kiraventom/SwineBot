using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwineBot;

public static class Common
{
    public static JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true, AllowTrailingCommas = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static CultureInfo RuCulture { get; } = CultureInfo.GetCultureInfo("ru");
}

