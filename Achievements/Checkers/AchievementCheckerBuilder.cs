using Serilog;
using SwineBot.Model;
using SwineBot.Text;

namespace SwineBot.Achievements.Checkers;

public class AchievementCheckerBuilder
{
    private AchievementType _type = AchievementType.None;
    private string _descriptionFormat;
    private Unit _levelUnit;
    private readonly List<AchievementLevel> _levels = [];

    public AchievementCheckerBuilder Type(AchievementType type)
    {
        if (_type != AchievementType.None)
        {
            Log.Error("Attempted to set type twice: old type {old}, new type {new}", _type.ToString(), type.ToString());
            return this;
        }

        _type = type;
        return this;
    }

    public AchievementCheckerBuilder Description(string descriptionFormat, Unit unit)
    {
        if (_descriptionFormat != null)
        {
            Log.Error("Attempted to set description format twice: old format {old}, new format {new}", _descriptionFormat, descriptionFormat);
            return this;
        }

        _descriptionFormat = descriptionFormat;
        _levelUnit = unit;
        return this;
    }

    public AchievementCheckerBuilder AddLevel(int value, string name)
    {
        if (_type == AchievementType.None)
        {
            Log.Error("Attempted to add level before setting type");
            return this;
        }

        int absValue = Math.Abs(value);
        var levelUnitDeclination = MessageTextUtils.GetDeclinatedNoun(absValue, _levelUnit);
        var description = string.Format(_descriptionFormat, absValue, levelUnitDeclination);
        _levels.Add(new AchievementLevel(value, name, description));
        return this;
    }

    public AchievementChecker Build()
    {
        _levels.Sort((a, b) => -1 * Math.Abs(a.Value).CompareTo(Math.Abs(b.Value)));
        return _type switch
        {
            AchievementType.Weight => new WeightAchievementChecker(_levels),
            AchievementType.WeightGain => new WeightGainAchievementChecker(_levels),
            AchievementType.WeightLoss => new WeightLossAchievementChecker(_levels),
            AchievementType.Overfeed => new OverfeedAchievementChecker(_levels),
            AchievementType.NoOverfeed => new NoOverfeedAchievementChecker(_levels),
            _ => new InvalidAchievementChecker(),
        };
    }
}

