namespace TaxiDispatch.Core.Legacy;

// Успадкований документ з методички (лістинг 4.3): усі властивості з публічним set.
public class LegacyTaxiOrder
{
    public int Id { get; set; }

    public int Status { get; set; }

    public decimal Total { get; set; }

    public string Email { get; set; } = string.Empty;

    public List<LegacySegment> Lines { get; set; } = new();

    public virtual void AddLine(LegacySegment line) => Lines.Add(line);
}
