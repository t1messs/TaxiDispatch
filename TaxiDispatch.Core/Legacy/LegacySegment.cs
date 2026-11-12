namespace TaxiDispatch.Core.Legacy;

// Успадкована позиція документа з методички (лістинг 4.3).
public class LegacySegment
{
    public string Destination { get; set; } = string.Empty;

    public int Distance { get; set; }

    public decimal PricePerKm { get; set; }
}
