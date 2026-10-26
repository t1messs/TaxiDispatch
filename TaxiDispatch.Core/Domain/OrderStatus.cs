namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Статуси замовлення таксі.
/// </summary>
public enum OrderStatus
{
    /// <summary>Нове замовлення.</summary>
    New,

    /// <summary>Призначено водія.</summary>
    Assigned,

    /// <summary>У дорозі.</summary>
    InProgress,

    /// <summary>Виконано.</summary>
    Completed,

    /// <summary>Скасовано.</summary>
    Cancelled
}
