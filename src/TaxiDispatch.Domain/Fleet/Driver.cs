using TaxiDispatch.Domain.Errors;
using TaxiDispatch.Domain.Geo;

namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Водій на лінії: стан і поточну позицію змінюють лише методи-переходи.
/// </summary>
public sealed class Driver
{
    /// <summary>
    /// Створює водія з відомим станом і позицією (зокрема під час завантаження з файлу).
    /// </summary>
    /// <param name="profile">Незмінні відомості про водія.</param>
    /// <param name="location">Поточна позиція водія.</param>
    /// <param name="status">Поточний стан водія.</param>
    /// <exception cref="ArgumentNullException">Профіль або позицію не передано.</exception>
    public Driver(DriverProfile profile, GeoPoint location, DriverStatus status)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(location);

        Profile = profile;
        Location = location;
        Status = status;
    }

    /// <summary>Отримує незмінні відомості про водія.</summary>
    public DriverProfile Profile { get; }

    /// <summary>Отримує номер водія.</summary>
    public int Id => Profile.Id;

    /// <summary>Отримує позицію водія; оновлюється лише після поїздки.</summary>
    public GeoPoint Location { get; private set; }

    /// <summary>Отримує поточний стан водія.</summary>
    public DriverStatus Status { get; private set; }

    /// <summary>
    /// Закріплює за водієм замовлення: стан Free → Busy.
    /// </summary>
    /// <exception cref="DomainRuleException">Водій не вільний.</exception>
    public void TakeOrder()
    {
        if (Status != DriverStatus.Free)
        {
            throw new DomainRuleException("driver.free", $"Водій {Id} не вільний: стан {Status}");
        }

        Status = DriverStatus.Busy;
    }

    /// <summary>
    /// Звільняє водія після поїздки чи скасування й переносить його в задану точку.
    /// </summary>
    /// <param name="location">Точка, де водій тепер чекає на замовлення.</param>
    /// <exception cref="DomainRuleException">Водій не виконує замовлення.</exception>
    public void Release(GeoPoint location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (Status != DriverStatus.Busy)
        {
            throw new DomainRuleException(
                "driver.busy",
                $"Водій {Id} не має замовлення: стан {Status}");
        }

        // TODO(TD-12): позиція оновлюється лише тут, після поїздки, бо GPS поза межами
        // системи (специфікація, 1); строк — разом зі сценарієм відстеження водіїв.
        Status = DriverStatus.Free;
        Location = location;
    }
}
