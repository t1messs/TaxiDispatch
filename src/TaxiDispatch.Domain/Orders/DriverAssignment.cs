namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Призначення водія на замовлення.
/// </summary>
public sealed record DriverAssignment
{
    /// <summary>
    /// Створює запис про призначення.
    /// </summary>
    /// <param name="driverId">Номер водія, більший за нуль.</param>
    /// <param name="pickupKm">Відстань від водія до подачі, км, не від'ємна.</param>
    /// <param name="assignedAt">Час призначення.</param>
    public DriverAssignment(int driverId, decimal pickupKm, DateTimeOffset assignedAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(driverId);
        ArgumentOutOfRangeException.ThrowIfNegative(pickupKm);

        DriverId = driverId;
        PickupKm = pickupKm;
        AssignedAt = assignedAt;
    }

    /// <summary>Отримує номер водія.</summary>
    public int DriverId { get; }

    /// <summary>Отримує відстань подачі, км.</summary>
    public decimal PickupKm { get; }

    /// <summary>Отримує час призначення.</summary>
    public DateTimeOffset AssignedAt { get; }
}
