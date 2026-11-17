namespace TaxiDispatch.Domain.Geo;

/// <summary>
/// Оцінка довжини маршруту дорогами: відстань по прямій × коефіцієнт доріг.
/// </summary>
public static class RoadDistance
{
    /// <summary>У скільки разів шлях дорогами довший за відстань по прямій.</summary>
    public const decimal RoadFactor = 1.3m;

    /// <summary>
    /// Оцінює довжину маршруту між точками. Не має побічних ефектів.
    /// </summary>
    /// <param name="from">Точка подачі.</param>
    /// <param name="to">Точка призначення.</param>
    /// <returns>Довжина маршруту, км, округлена до метра.</returns>
    public static decimal Estimate(GeoPoint from, GeoPoint to) => Math.Round(
        GeoDistance.KilometersBetween(from, to) * RoadFactor,
        GeoDistance.DistanceDecimals,
        MidpointRounding.AwayFromZero);
}
