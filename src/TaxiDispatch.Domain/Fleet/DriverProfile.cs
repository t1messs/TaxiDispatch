namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Незмінні відомості про водія: номер, ім'я, автомобіль і рейтинг.
/// </summary>
public sealed record DriverProfile
{
    /// <summary>Найнижчий допустимий рейтинг водія.</summary>
    public const decimal MinRating = 1m;

    /// <summary>Найвищий допустимий рейтинг водія.</summary>
    public const decimal MaxRating = 5m;

    /// <summary>
    /// Створює профіль водія й перевіряє його інваріант.
    /// </summary>
    /// <param name="id">Номер водія, більший за нуль.</param>
    /// <param name="name">Ім'я та прізвище, не порожні.</param>
    /// <param name="vehicle">Автомобіль, закріплений за водієм.</param>
    /// <param name="rating">Рейтинг від <see cref="MinRating"/> до <see cref="MaxRating"/>.</param>
    /// <exception cref="ArgumentException">Порушено інваріант профілю.</exception>
    public DriverProfile(int id, string name, Vehicle vehicle, decimal rating)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentOutOfRangeException.ThrowIfLessThan(rating, MinRating);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rating, MaxRating);

        Id = id;
        Name = name;
        Vehicle = vehicle;
        Rating = rating;
    }

    /// <summary>Отримує номер водія.</summary>
    public int Id { get; }

    /// <summary>Отримує ім'я та прізвище водія.</summary>
    public string Name { get; }

    /// <summary>Отримує автомобіль водія.</summary>
    public Vehicle Vehicle { get; }

    /// <summary>Отримує рейтинг водія; за рівної відстані перевагу має вищий.</summary>
    public decimal Rating { get; }
}
