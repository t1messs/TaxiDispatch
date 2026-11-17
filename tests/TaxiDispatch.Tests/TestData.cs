using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Geo;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Places;
using TaxiDispatch.Domain.Pricing;

namespace TaxiDispatch.Tests;

// Спільні зразки даних для тестів. Тарифи — специфікація, таблиця 4.1.
// Точки вздовж меридіана 30,5: 0,01° широти = 1,112 км по прямій.
internal static class TestData
{
    public static readonly GeoPoint Center = new(50.0, 30.5);

    public static readonly Place Station = new("P01", "Вокзал", Center);

    public static readonly Place Airport = new("P02", "Аеропорт", new GeoPoint(50.05, 30.55));

    public static TariffBook Tariffs() => new(
    [
        new Tariff(CarClass.Economy, 40m, 12m, 90m),
        new Tariff(CarClass.Comfort, 55m, 16m, 120m),
        new Tariff(CarClass.Business, 90m, 26m, 220m),
    ]);

    public static GeoPoint North(double latitudeDegrees) =>
        new(Center.Latitude + latitudeDegrees, Center.Longitude);

    public static Driver FreeDriver(
        int id,
        GeoPoint location,
        CarClass carClass = CarClass.Economy,
        decimal rating = 4.5m)
    {
        var vehicle = new Vehicle("AA" + id + "BC", "Skoda Octavia", carClass);
        return new Driver(
            new DriverProfile(id, "Водій " + id, vehicle, rating),
            location,
            DriverStatus.Free);
    }

    public static TaxiOrder NewOrder(int id = 1, CarClass carClass = CarClass.Economy) =>
        new(id, 1, new TripPlan(Station, Airport, carClass), At(12));

    public static DateTimeOffset At(int hour, int day = 17) =>
        new(2026, 11, day, hour, 0, 0, TimeSpan.FromHours(2));
}
