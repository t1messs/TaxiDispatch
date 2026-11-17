using TaxiDispatch.Domain.Geo;

namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Добір водія за правилом 4.2 специфікації: найближчий вільний водій потрібного класу.
/// </summary>
public static class DriverMatcher
{
    /// <summary>Найбільша відстань подачі, км; межа включно.</summary>
    public const decimal MaxPickupKm = 5m;

    /// <summary>
    /// Обирає водія: вільний, потрібного класу, не далі <see cref="MaxPickupKm"/> від подачі;
    /// серед них — найближчий, за рівної відстані — з вищим рейтингом, далі — з меншим номером.
    /// Не має побічних ефектів.
    /// </summary>
    /// <param name="drivers">Усі водії автопарку.</param>
    /// <param name="pickup">Точка подачі.</param>
    /// <param name="carClass">Потрібний клас автомобіля.</param>
    /// <returns>Обраний водій із відстанню подачі або <c>null</c>, якщо кандидатів немає.</returns>
    /// <exception cref="ArgumentNullException">Список водіїв або точку не передано.</exception>
    public static DriverMatch? Choose(
        IEnumerable<Driver> drivers,
        GeoPoint pickup,
        CarClass carClass)
    {
        ArgumentNullException.ThrowIfNull(drivers);
        ArgumentNullException.ThrowIfNull(pickup);

        return drivers
            .Where(driver => driver.Status == DriverStatus.Free)
            .Where(driver => driver.Profile.Vehicle.CarClass == carClass)
            .Select(driver => new DriverMatch(
                driver,
                GeoDistance.KilometersBetween(driver.Location, pickup)))
            .Where(match => match.PickupKm <= MaxPickupKm)
            .OrderBy(match => match.PickupKm)
            .ThenByDescending(match => match.Driver.Profile.Rating)
            .ThenBy(match => match.Driver.Id)
            .FirstOrDefault();
    }
}
