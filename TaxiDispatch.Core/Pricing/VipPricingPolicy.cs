using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Pricing;

/// <summary>
/// Політика для VIP-пасажирів: окреме правило — окремий клас, наявні класи не змінюються.
/// </summary>
public sealed class VipPricingPolicy : IPricingPolicy
{
    private const decimal VipDiscountRate = 0.10m;

    /// <inheritdoc/>
    public decimal PriceOf(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        decimal price = segment.Amount * (1m - VipDiscountRate);
        return Math.Round(price, 2, MidpointRounding.AwayFromZero);
    }
}
