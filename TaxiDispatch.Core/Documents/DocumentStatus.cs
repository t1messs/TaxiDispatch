namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Стан документа розрахунку.
/// </summary>
public enum DocumentStatus
{
    /// <summary>Документ створено, оплати ще немає.</summary>
    New,

    /// <summary>Документ оплачено.</summary>
    Paid,

    /// <summary>Поїздку виконано за оплаченим документом.</summary>
    Shipped,

    /// <summary>Документ скасовано, сума дорівнює нулю.</summary>
    Cancelled,
}
