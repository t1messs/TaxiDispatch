namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Підсумок водія за зміну: кількість поїздок і виручка.
/// </summary>
public sealed record DriverTotal
{
    /// <summary>
    /// Створює підсумок водія.
    /// </summary>
    /// <param name="driverName">Ім'я водія.</param>
    /// <param name="rides">Кількість завершених поїздок.</param>
    /// <param name="revenue">Виручка, грн.</param>
    public DriverTotal(string driverName, int rides, decimal revenue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driverName);

        DriverName = driverName;
        Rides = rides;
        Revenue = revenue;
    }

    /// <summary>Отримує ім'я водія.</summary>
    public string DriverName { get; }

    /// <summary>Отримує кількість поїздок.</summary>
    public int Rides { get; }

    /// <summary>Отримує виручку, грн.</summary>
    public decimal Revenue { get; }
}
