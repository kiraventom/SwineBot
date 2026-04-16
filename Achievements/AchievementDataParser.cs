using SwineBot.Model;
using SwineBot.Achievements.Effects;
using SwineBot.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SwineBot.Achievements.Checkers;

public class AchievementDataParser(ILogger<AchievementDataParser> logger, AchievementEffectFactory effectFactory)
{
    public IReadOnlyCollection<AchievementData> ParseJson(string jsonPath)
    {
        JsonElement root;

        if (!File.Exists(jsonPath))
        {
            logger.LogError("File \"{path}\" does not exist", jsonPath);
            return [];
        }

        try
        {
            using (var json = File.OpenRead(jsonPath))
            {
                var document = JsonDocument.Parse(json);
                root = document.RootElement;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open file \"{path}\"", jsonPath);
            return [];
        }

        var datas = new List<AchievementData>();
        int index = -1;

        foreach (var el in root.EnumerateArray())
        {
            ++index;

            AchievementData data;

            try
            {
                data = ParseElement(el);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse element at index \"{i}\"", index);
                continue;
            }

            datas.Add(data);
        }

        return datas;
    }

    private AchievementData ParseElement(JsonElement el)
    {
        var typeStr = el.GetProperty(nameof(AchievementData.Type)).GetString();
        var type = Enum.Parse<AchievementType>(typeStr);

        var descFormat = el.GetProperty("DescriptionFormat").GetString();

        var unitStr = el.GetProperty("DescriptionUnit").GetString();
        var unit = Unit.FindByName(unitStr);

        bool isArchived = false;
        if (el.TryGetProperty(nameof(AchievementData.IsArchived), out var isArchivedProp))
            isArchived = isArchivedProp.GetBoolean();

        List<AchievementLevel> levels = [];

        var levelElements = el.GetProperty(nameof(AchievementData.Levels)).EnumerateArray();
        foreach (var levelEl in levelElements)
        {
            var name = levelEl.GetProperty(nameof(AchievementLevel.Name)).GetString();
            var value = levelEl.GetProperty(nameof(AchievementLevel.Value)).GetInt32();
            
            IAchievementEffect effect = null;

            if (levelEl.TryGetProperty(nameof(AchievementLevel.Effect), out var effectEl))
            {
                var effectTypeStr = effectEl.GetProperty(nameof(IAchievementEffect.Type)).GetString();
                var effectType = Enum.Parse<AchievementEffectType>(effectTypeStr);

                var effectValue = effectEl.GetProperty("Value").GetDouble();

                effect = effectFactory.Build(effectType, effectValue);
            }

            var description = BuildAchievementLevelDescription(descFormat, value, unit);
            var level = new AchievementLevel(value, name, description, effect);
            levels.Add(level);
        }

        var data = new AchievementData(type, levels, isArchived);
        return data;
    }

    private static string BuildAchievementLevelDescription(string descriptionFormat, int value, Unit unit)
    {
        int absValue = Math.Abs(value);
        var levelUnitDeclination = MessageTextUtils.GetDeclinatedNoun(absValue, unit);
        var description = string.Format(descriptionFormat, absValue, levelUnitDeclination);
        return description;
    }
}

public record AchievementData(AchievementType Type, IReadOnlyCollection<AchievementLevel> Levels, bool IsArchived);
