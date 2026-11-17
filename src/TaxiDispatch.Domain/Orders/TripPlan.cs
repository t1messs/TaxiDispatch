using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Geo;
using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Що замовив пасажир: звідки, куди й яким класом автомобіля.
/// </summary>
public sealed record TripPlan
{
    /// <summary>
    /// Створює план поїздки.
    /// </summary>
    /// <param name="pickup">Місце подачі.</param>
    /// <param name="dropoff">Місце призначення.</param>
    /// <param name="carClass">Клас автомобіля.</param>
    public TripPlan(Place pickup, Place dropoff, CarClass carClass)
    {
        ArgumentNullException.ThrowIfNull(pickup);
        ArgumentNullException.ThrowIfNull(dropoff);

        Pickup = pickup;
        Dropoff = dropoff;
        CarClass = carClass;
    }

    /// <summary>Отримує місце подачі.</summary>
    public Place Pickup { get; }

    /// <summary>Отримує місце призначення.</summary>
    public Place Dropoff { get; }

    /// <summary>Отримує клас автомобіля.</summary>
    public CarClass CarClass { get; }

    /// <summary>Отримує оцінку довжини маршруту дорогами, км.</summary>
    public decimal RouteKm => RoadDistance.Estimate(Pickup.Location, Dropoff.Location);
}
