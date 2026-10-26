namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Відрізок маршруту.
/// </summary>
public class RouteSegment
{
    /// <summary>Отримує або задає адресу призначення.</summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>Отримує або задає дистанцію в кілометрах.</summary>
    public decimal DistanceKm { get; set; }

    /// <summary>Отримує або задає ціну за кілометр.</summary>
    public decimal PricePerKm { get; set; }

    /// <summary>Отримує загальну вартість відрізка.</summary>
    public decimal Amount => DistanceKm * PricePerKm;
}