namespace TaxiDispatch.Domain.Geo;

/// <summary>
/// Відстань між географічними точками по поверхні Землі (формула гаверсинусів).
/// </summary>
public static class GeoDistance
{
    /// <summary>Середній радіус Землі за IUGG, км.</summary>
    public const double EarthRadiusKm = 6371.0088;

    /// <summary>Знаків після коми у відстані, км: три знаки — точність до 1 м (X-02).</summary>
    public const int DistanceDecimals = 3;

    private const double DegreesInHalfTurn = 180.0;

    /// <summary>
    /// Обчислює відстань між двома точками по дузі великого кола. Не має побічних ефектів.
    /// </summary>
    /// <param name="from">Початкова точка.</param>
    /// <param name="to">Кінцева точка.</param>
    /// <returns>Відстань, км, округлена до метра.</returns>
    /// <exception cref="ArgumentNullException">Точку не передано.</exception>
    public static decimal KilometersBetween(GeoPoint from, GeoPoint to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        double fromLatitude = ToRadians(from.Latitude);
        double toLatitude = ToRadians(to.Latitude);
        double sinHalfLatitude = Math.Sin((toLatitude - fromLatitude) / 2);
        double sinHalfLongitude = Math.Sin(ToRadians(to.Longitude - from.Longitude) / 2);

        // h = sin²(Δφ/2) + cos φ1 · cos φ2 · sin²(Δλ/2); d = 2R · arcsin(√h).
        // Math.Min захищає arcsin від h = 1,0000000001 через похибку double.
        double haversine = (sinHalfLatitude * sinHalfLatitude)
            + (Math.Cos(fromLatitude) * Math.Cos(toLatitude) * sinHalfLongitude * sinHalfLongitude);
        double centralAngle = 2 * Math.Asin(Math.Sqrt(Math.Min(1.0, haversine)));

        // Єдине місце переходу double → decimal (X-04).
        decimal distanceKm = (decimal)(EarthRadiusKm * centralAngle);
        return Math.Round(distanceKm, DistanceDecimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Перевіряє, чи лежать точки не далі заданого допуску (X-05). Не має побічних ефектів.
    /// </summary>
    /// <param name="first">Перша точка.</param>
    /// <param name="second">Друга точка.</param>
    /// <param name="toleranceKm">Допуск, км; не від'ємний.</param>
    /// <returns><c>true</c>, якщо відстань між точками не більша за допуск.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Допуск від'ємний.</exception>
    public static bool IsNear(GeoPoint first, GeoPoint second, decimal toleranceKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(toleranceKm);
        return KilometersBetween(first, second) <= toleranceKm;
    }

    private static double ToRadians(double degrees) => (degrees * Math.PI) / DegreesInHalfTurn;
}
