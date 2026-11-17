namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Результат добору: обраний водій і його відстань до точки подачі.
/// </summary>
public sealed record DriverMatch
{
    /// <summary>
    /// Створює результат добору.
    /// </summary>
    /// <param name="driver">Обраний водій.</param>
    /// <param name="pickupKm">Відстань від водія до точки подачі, км, не від'ємна.</param>
    public DriverMatch(Driver driver, decimal pickupKm)
    {
        ArgumentNullException.ThrowIfNull(driver);
        ArgumentOutOfRangeException.ThrowIfNegative(pickupKm);

        Driver = driver;
        PickupKm = pickupKm;
    }

    /// <summary>Отримує обраного водія.</summary>
    public Driver Driver { get; }

    /// <summary>Отримує відстань подачі, км.</summary>
    public decimal PickupKm { get; }
}
