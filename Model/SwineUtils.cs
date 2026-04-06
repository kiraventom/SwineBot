namespace SwineBot.Model;

public static class SwineUtils
{
    private const int SHORT_NAME_LENGTH = 20;

    public static string GetShortName(Swine swine)
    {
        const char ellipsis = '\u2026';
        string swineName = swine.Name;
        if (swineName.Length > SHORT_NAME_LENGTH)
            swineName = $"{swineName[..(SHORT_NAME_LENGTH - 1)]}{ellipsis}";

        return swineName;
    }
}


