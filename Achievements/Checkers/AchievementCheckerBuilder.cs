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

    private bool _isSealed = false;

    public AchievementType CheckerType => _type;

    public AchievementCheckerBuilder Type(AchievementType type)
    {
        CheckForSealed();
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
        CheckForSealed();
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
        CheckForSealed();
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

    public AchievementCheckerBuilder Seal()
    {
        CheckForSealed();
        _levels.Sort((a, b) => -1 * Math.Abs(a.Value).CompareTo(Math.Abs(b.Value)));
        _isSealed = true;
        return this;
    }

    public AchievementChecker Build(BuildCheckerDelegate BuildChecker)
    {
        return _type switch
        {
            AchievementType.Weight => BuildChecker(typeof(WeightAchievementChecker), _levels),
            AchievementType.WeightGain => BuildChecker(typeof(WeightGainAchievementChecker), _levels),
            AchievementType.WeightLoss => BuildChecker(typeof(WeightLossAchievementChecker), _levels),
            AchievementType.Overfeed => BuildChecker(typeof(OverfeedAchievementChecker), _levels),
            AchievementType.NoOverfeed => BuildChecker(typeof(NoOverfeedAchievementChecker), _levels),
            _ => BuildChecker(typeof(InvalidAchievementChecker), null),
        };
    }

    private void CheckForSealed()
    {
        if (_isSealed)
            throw new NotSupportedException("Can't modify sealed builder");
    }
}

