using TaxiDispatch.Domain.Geo;

namespace TaxiDispatch.Domain.Places;

/// <summary>
/// Місце подачі або призначення: код довідника, назва й координати.
/// </summary>
public sealed record Place
{
    /// <summary>
    /// Створює місце й перевіряє обов'язкові поля.
    /// </summary>
    /// <param name="code">Код місця, наприклад «P07»; для довільних координат — «GPS».</param>
    /// <param name="name">Назва для людини, не порожня.</param>
    /// <param name="location">Координати місця.</param>
    /// <exception cref="ArgumentException">Код або назва порожні.</exception>
    public Place(string code, string name, GeoPoint location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(location);

        Code = code;
        Name = name;
        Location = location;
    }

    /// <summary>Отримує код місця.</summary>
    public string Code { get; }

    /// <summary>Отримує назву місця.</summary>
    public string Name { get; }

    /// <summary>Отримує координати місця.</summary>
    public GeoPoint Location { get; }
}
