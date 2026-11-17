namespace TaxiDispatch.Core.Legacy;

/// <summary>
/// Позиція документа в модулі документів (перенесено з ЛР 5; борг TD-10).
/// </summary>
public sealed class RouteSegment
{
    /// <summary>Отримує або задає адресу призначення.</summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>Отримує або задає відстань, км (ціле число — порушення X-02).</summary>
    public int Distance { get; set; }

    /// <summary>Отримує або задає ціну за кілометр, грн.</summary>
    public decimal PricePerKm { get; set; }
}
