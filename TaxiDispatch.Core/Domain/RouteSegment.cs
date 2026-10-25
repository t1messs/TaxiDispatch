namespace TaxiDispatch.Core.Domain;

/// <summary>Відрізок маршруту: адреса, відстань і ціна за кілометр.</summary>
public sealed class RouteSegment
{
    public string DestinationAddress { get; init; } = string.Empty;

    public decimal DistanceKm { get; init; }

    public decimal PricePerKm { get; init; }

    public decimal Amount => DistanceKm * PricePerKm;
}
