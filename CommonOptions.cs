using System.Text.Json;

namespace SwineBot;

public static class CommonOptions
{
   public static JsonSerializerOptions Json = new()
   {
      WriteIndented = true, AllowTrailingCommas = true
   };
}

