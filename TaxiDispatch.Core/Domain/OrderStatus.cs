namespace TaxiDispatch.Core.Domain;

/// <summary>Стан замовлення таксі в його життєвому циклі.</summary>
public enum OrderStatus
{
    New = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
