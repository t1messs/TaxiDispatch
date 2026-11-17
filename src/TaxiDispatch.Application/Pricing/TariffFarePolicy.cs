using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Pricing;

namespace TaxiDispatch.Application.Pricing;

/// <summary>
/// Вартість за тарифною сіткою з нічною надбавкою (специфікація, правило 4.1).
/// </summary>
public sealed class TariffFarePolicy : IFarePolicy
{
    private readonly TariffBook _tariffs;

    /// <summary>
    /// Створює політику з тарифною сіткою.
    /// </summary>
    /// <param name="tariffs">Тарифи всіх класів автомобілів.</param>
    public TariffFarePolicy(TariffBook tariffs)
    {
        _tariffs = tariffs ?? throw new ArgumentNullException(nameof(tariffs));
    }

    /// <inheritdoc/>
    public decimal Quote(CarClass carClass, decimal distanceKm, DateTimeOffset pickupTime)
    {
        bool isNight = FareCalculator.IsNight(TimeOnly.FromDateTime(pickupTime.DateTime));
        return FareCalculator.Calculate(_tariffs.For(carClass), distanceKm, isNight);
    }
}
