using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Abstractions;

/// <summary>
/// Політика розрахунку ціни відрізка маршруту.
/// </summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює ціну відрізка маршруту за правилом політики.</summary>
    /// <param name="segment">Відрізок маршруту.</param>
    /// <returns>Ціна відрізка, заокруглена до копійок.</returns>
    decimal PriceOf(RouteSegment segment);
}
