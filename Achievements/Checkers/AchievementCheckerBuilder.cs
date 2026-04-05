using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwineBot.Achievements.Effects;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.Achievements.Checkers;

public class AchievementCheckerBuilder(ILogger<AchievementCheckerBuilder> logger)
{
    private AchievementType _type = AchievementType.None;
    private string _descriptionFormat;
    private Unit _levelUnit;
    private readonly List<AchievementLevel> _levels = [];

    public AchievementType CheckerType => _type;

    public AchievementCheckerBuilder Type(AchievementType type)
    {
        if (_type != AchievementType.None)
        {
            logger.LogError("Attempted to set type twice: old type {old}, new type {new}", _type.ToString(), type.ToString());
            return this;
        }

        _type = type;
        return this;
    }

    public AchievementCheckerBuilder Description(string descriptionFormat, Unit unit)
    {
        if (_descriptionFormat != null)
        {
            logger.LogError("Attempted to set description format twice: old format {old}, new format {new}", _descriptionFormat, descriptionFormat);
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
            logger.LogError("Attempted to add level before setting type");
            return this;
        }

        int absValue = Math.Abs(value);
        var levelUnitDeclination = MessageTextUtils.GetDeclinatedNoun(absValue, _levelUnit);
        var description = string.Format(_descriptionFormat, absValue, levelUnitDeclination);
        _levels.Add(new AchievementLevel(value, name, description, effect));
        return this;
    }

    public AchievementChecker Build(IServiceProvider sp)
    {
        _levels.Sort((a, b) => -1 * Math.Abs(a.Value).CompareTo(Math.Abs(b.Value)));
        return _type switch
        {
            AchievementType.Weight => ActivatorUtilities.CreateInstance<WeightAchievementChecker>(sp, _levels),
            AchievementType.WeightGain => ActivatorUtilities.CreateInstance<WeightGainAchievementChecker>(sp, _levels),
            AchievementType.WeightLoss => ActivatorUtilities.CreateInstance<WeightLossAchievementChecker>(sp, _levels),
            AchievementType.Overfeed => ActivatorUtilities.CreateInstance<OverfeedAchievementChecker>(sp, _levels),
            AchievementType.NoOverfeed => ActivatorUtilities.CreateInstance<NoOverfeedAchievementChecker>(sp, _levels),
            _ => ActivatorUtilities.CreateInstance<InvalidAchievementChecker>(null),
        };
    }
}

