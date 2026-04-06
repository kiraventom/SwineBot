using SwineBot.Model;
using SwineBot.Achievements.Effects;

namespace SwineBot.Achievements.Checkers;

public interface IAchievementCheckerBuilders
{
    AchievementCheckerBuilder Get(AchievementType type);
    IEnumerable<AchievementCheckerBuilder> GetAll();
}

// TODO JSONLEVELS: Move levels to json file
public class AchievementCheckerBuilders : IAchievementCheckerBuilders
{
    private readonly Dictionary<AchievementType, AchievementCheckerBuilder> _checkerBuilders;

    public AchievementCheckerBuilders(IAchievementCheckerBuilderFactory factory)
    {
        List<AchievementCheckerBuilder> checkerBuilders = 
        [
            factory.Create()
                .Type(AchievementType.Weight)
                .Description("Набрать {0} {1}", Unit.Kg)
                .AddLevel(100, "Сотка")
                .AddLevel(228, "Да не торч я")
                .AddLevel(420, "Ладно я торч")
                .AddLevel(690, "Кевин")
                .AddLevel(777, "А потом поебалися")
                .AddLevel(812, "Беглов")
                .AddLevel(999, "Недобор")
                .AddLevel(1000, "Речь идёт о четырёхзначных числах")
                .AddLevel(1234, "Код от домофона")
                .AddLevel(1337, "}{@k3p")
                .AddLevel(1488, "Егор Просвинин")
                .AddLevel(1580, "Вертикаль")
                .AddLevel(1703, "Круче Петра")
                .AddLevel(1917, "Октябрёнок")
                .AddLevel(2000, "2K")
                .AddLevel(2007, "Вернулся")
                .AddLevel(2022, "Свой парень")
                .AddLevel(5000, "А хули вы хотели?")
                .Seal(),

            factory.Create()
                .Type(AchievementType.WeightGain)
                .Description("Поесть на {0} {1}", Unit.Kg)
                .AddLevel(1, "Заморил червячка")
                .AddLevel(22, "От пуза")
                .Seal(),

            factory.Create()
                .Type(AchievementType.WeightLoss)
                .Description("Похудеть на {0} {1}", Unit.Kg)
                .AddLevel(-1, "И не заметил")
                .AddLevel(-20, "Серьёзный ущерб")
                .AddLevel(-40, "Жадность фраера сгубила")
                .AddLevel(-60, "he bought", new ThrowupScaleEffect(0.95))
                .AddLevel(-80, "После такого не встают", new ThrowupScaleEffect(0.85))
                .AddLevel(-100, "Булимия", new ThrowupScaleEffect(0.70))
                .Seal(),

            factory.Create()
                .Type(AchievementType.Overfeed)
                .Description("Успешный перекорм {0} {1} подряд", Unit.Time)
                .AddLevel(3, "Завтрак, обед, ужин")
                .AddLevel(5, "Плюс полдник и ланч")
                .AddLevel(7, "Недельный рацион")
                .AddLevel(14, "Двухнедельный рацион", new ThrowupIgnoreChanceEffect(0.25))
                .AddLevel(21, "Трёхнедельный рацион", new ThrowupIgnoreChanceEffect(0.50))
                .AddLevel(31, "Месячная порция", new ThrowupIgnoreChanceEffect(0.75))
                .Seal(),

            factory.Create()
                .Type(AchievementType.NoOverfeed)
                .Description("Без перекорма {0} {1} подряд", Unit.Time)
                .AddLevel(3, "Пивная диета")
                .AddLevel(5, "Йогуртовая диета")
                .AddLevel(7, "Яблочная диета")
                .AddLevel(14, "Водная диета")
                .AddLevel(21, "Диета Типичной Анорексички")
                .AddLevel(31, "Диета Ларисы Долиной", new NoOverfeedsLuckAmplifierEffect())
                .Seal(),
        ];

        _checkerBuilders = checkerBuilders.ToDictionary(b => b.CheckerType);
    }

    public AchievementCheckerBuilder Get(AchievementType type) => _checkerBuilders[type];
    public IEnumerable<AchievementCheckerBuilder> GetAll() => _checkerBuilders.Values;
}

