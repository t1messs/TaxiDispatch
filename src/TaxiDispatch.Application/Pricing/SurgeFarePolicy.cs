using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Pricing;

namespace TaxiDispatch.Application.Pricing;

/// <summary>
/// Коефіцієнт попиту поверх будь-якої політики вартості (декоратор). Функція вже в main,
/// але діє лише тоді, коли її ввімкнено прапорцем у композиційному корені.
/// </summary>
public sealed class SurgeFarePolicy : IFarePolicy
{
    /// <summary>Найменший коефіцієнт: вартість без надбавки.</summary>
    public const decimal MinFactor = 1m;

    /// <summary>Найбільший коефіцієнт попиту, який дозволяє служба.</summary>
    public const decimal MaxFactor = 2m;

    private readonly IFarePolicy _inner;
    private readonly decimal _factor;

    /// <summary>
    /// Створює політику з коефіцієнтом попиту.
    /// </summary>
    /// <param name="inner">Політика, вартість якої множиться на коефіцієнт.</param>
    /// <param name="factor">
    /// Коефіцієнт від <see cref="MinFactor"/> до <see cref="MaxFactor"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Коефіцієнт поза межами.</exception>
    public SurgeFarePolicy(IFarePolicy inner, decimal factor)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        ArgumentOutOfRangeException.ThrowIfLessThan(factor, MinFactor);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(factor, MaxFactor);
        _factor = factor;
    }

    /// <inheritdoc/>
    public decimal Quote(CarClass carClass, decimal distanceKm, DateTimeOffset pickupTime)
    {
        decimal fare = _inner.Quote(carClass, distanceKm, pickupTime) * _factor;
        return Math.Round(fare, FareCalculator.MoneyDecimals, MidpointRounding.AwayFromZero);
    }
}
