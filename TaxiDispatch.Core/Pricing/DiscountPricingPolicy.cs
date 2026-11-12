using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Pricing;

/// <summary>
/// Політика зі знижкою: ціна відрізка зменшується на задану частку.
/// </summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    private const decimal MaxRate = 0.5m;

    private readonly decimal _rate;

    /// <summary>
    /// Створює політику зі знижкою.
    /// </summary>
    /// <param name="rate">Частка знижки від 0 до 0,5.</param>
    public DiscountPricingPolicy(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, MaxRate);

        _rate = rate;
    }

    /// <inheritdoc/>
    public decimal PriceOf(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        return Math.Round(segment.Amount * (1m - _rate), 2);
    }
}
