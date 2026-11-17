namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Результат розрахунку документа.
/// </summary>
/// <param name="Total">Сума до сплати, грн, округлена до копійок.</param>
/// <param name="Status">Стан документа після розрахунку.</param>
public sealed record TotalResult(
    decimal Total,
    DocumentStatus Status);
