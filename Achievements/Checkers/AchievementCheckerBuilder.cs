using Microsoft.Extensions.Logging;
using SwineBot.Achievements.Effects;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.Achievements.Checkers;

public class AchievementCheckerBuilder(ILogger<AchievementCheckerBuilder> Logger, IAchievementCheckerFactory CheckerFactory)
{
    private AchievementType _type = AchievementType.None;
    private string _descriptionFormat;
    private Unit _levelUnit;
    private readonly List<AchievementLevel> _levels = [];

    public AchievementCheckerBuilder Type(AchievementType type)
    {
        if (_type != AchievementType.None)
        {
            Logger.LogError("Attempted to set type twice: old type {old}, new type {new}", _type.ToString(), type.ToString());
            return this;
        }

        _type = type;
        return this;
    }

    public AchievementCheckerBuilder Description(string descriptionFormat, Unit unit)
    {
        if (_descriptionFormat != null)
        {
            Logger.LogError("Attempted to set description format twice: old format {old}, new format {new}", _descriptionFormat, descriptionFormat);
            return this;
        }

        _descriptionFormat = descriptionFormat;
        _levelUnit = unit;
        return this;
    }

    public AchievementCheckerBuilder AddLevel(int value, string name, IAchievementEffect effect = null)
    {
        if (_type == AchievementType.None)
        {
            Logger.LogError("Attempted to add level before setting type");
            return this;
        }

        int absValue = Math.Abs(value);
        var levelUnitDeclination = MessageTextUtils.GetDeclinatedNoun(absValue, _levelUnit);
        var description = string.Format(_descriptionFormat, absValue, levelUnitDeclination);
        _levels.Add(new AchievementLevel(value, name, description, effect));
        return this;
    }

    public AchievementChecker Build()
    {
        _levels.Sort((a, b) => -1 * Math.Abs(a.Value).CompareTo(Math.Abs(b.Value)));
        return _type switch
        {
            AchievementType.Weight => CheckerFactory.Create<WeightAchievementChecker>(_levels),
            AchievementType.WeightGain => CheckerFactory.Create<WeightGainAchievementChecker>(_levels),
            AchievementType.WeightLoss => CheckerFactory.Create<WeightLossAchievementChecker>(_levels),
            AchievementType.Overfeed => CheckerFactory.Create<OverfeedAchievementChecker>(_levels),
            AchievementType.NoOverfeed => CheckerFactory.Create<NoOverfeedAchievementChecker>(_levels),
            _ => CheckerFactory.Create<InvalidAchievementChecker>(null),
        };
    }
}

