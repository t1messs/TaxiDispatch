namespace TaxiDispatch.Core.Legacy;

public sealed class RouteSegment
{
    public string Destination { get; set; } = "";
    public int Distance { get; set; }
    public decimal PricePerKm { get; set; }
}
