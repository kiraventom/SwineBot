using System.Reflection;

namespace SwineBot.Achievements;

public record Unit(string Singular, string AccusativeSingular, string AccusativePlural)
{
    public static Unit Kg { get; } = new("килограмм", "килограмма", "килограмм");
    public static Unit Time { get; } = new("раз", "раза", "раз");
    public static Unit Hour { get; } = new("час", "часа", "часов");
    public static Unit Minute { get; } = new("минута", "минуты", "минут");
    public static Unit Win { get; } = new("победа", "победы", "побед");
    public static Unit Loss { get; } = new("поражение", "поражения", "поражений");
    public static Unit Meal { get; } = new("приём", "приёма", "приёмов");
    public static Unit Overfeed { get; } = new("перекорм", "перекорма", "перекормов");
    public static Unit Achievement { get; } = new("достижение", "достижения", "достижений");

    private static IReadOnlyDictionary<string, Unit> _cache;

    static Unit()
    {
        var unit = typeof(Unit);
        var props = unit.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.PropertyType == typeof(Unit));
        var dict = props.ToDictionary(p => p.Name, p => (Unit)p.GetValue(null));
        _cache = dict;
    }

    public static Unit FindByName(string name) => _cache.TryGetValue(name, out var unit) ? unit : null;

}

