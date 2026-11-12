namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Відрізок маршруту — незмінна позиція замовлення таксі.
/// </summary>
public sealed record RouteSegment
{
    /// <summary>
    /// Створює відрізок маршруту та перевіряє його інваріант.
    /// </summary>
    /// <param name="destination">Адреса кінцевого пункту відрізка.</param>
    /// <param name="distanceKm">Дистанція в кілометрах, більша за нуль.</param>
    /// <param name="pricePerKm">Ціна за кілометр, не менша за нуль.</param>
    public RouteSegment(string destination, decimal distanceKm, decimal pricePerKm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(distanceKm);
        ArgumentOutOfRangeException.ThrowIfNegative(pricePerKm);

        Destination = destination;
        DistanceKm = distanceKm;
        PricePerKm = pricePerKm;
    }

    /// <summary>Отримує адресу кінцевого пункту відрізка.</summary>
    public string Destination { get; }

    /// <summary>Отримує дистанцію відрізка в кілометрах.</summary>
    public decimal DistanceKm { get; }

    /// <summary>Отримує ціну за один кілометр.</summary>
    public decimal PricePerKm { get; }

    /// <summary>Отримує вартість відрізка; обчислюється, а не зберігається.</summary>
    public decimal Amount => DistanceKm * PricePerKm;
}
