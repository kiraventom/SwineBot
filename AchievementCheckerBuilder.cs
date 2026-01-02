using SwineBot.Model;

namespace SwineBot.Achievements;

public class AchievementCheckerBuilder
{
    private AchievementType _type = AchievementType.None;
    private string _descriptionFormat;
    private readonly List<AchievementLevel> _levels = [];

    public AchievementCheckerBuilder Type(AchievementType type)
    {
        if (_type != AchievementType.None)
            throw new NotSupportedException();

        _type = type;
        return this;
    }

    public AchievementCheckerBuilder Description(string descriptionFormat)
    {
        if (_descriptionFormat != null)
            throw new NotSupportedException();

        _descriptionFormat = descriptionFormat;
        return this;
    }

    public AchievementCheckerBuilder AddLevel(int value, string name)
    {
        if (_type == AchievementType.None)
            throw new NotSupportedException();

        var description = string.Format(_descriptionFormat, value);
        _levels.Add(new AchievementLevel(value, name, description));
        return this;
    }

    public AchievementChecker Build()
    {
        _levels.Sort((a, b) => -1 * a.Value.CompareTo(b.Value));
        return _type switch
        {
            AchievementType.Weight => new WeightAchievementChecker(_levels),
            AchievementType.WeightGain => new WeightGainAchievementChecker(_levels),
            AchievementType.WeightLoss => new WeightLossAchievementChecker(_levels),
            AchievementType.Overfeed => new OverfeedAchievementChecker(_levels),
            AchievementType.NoOverfeed => new NoOverfeedAchievementChecker(_levels),
        };
    }
}

