using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Pricing;

/// <summary>
/// Звичайна політика: ціна відрізка без знижок.
/// </summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    public decimal PriceOf(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        return Math.Round(segment.Amount, 2);
    }
}
