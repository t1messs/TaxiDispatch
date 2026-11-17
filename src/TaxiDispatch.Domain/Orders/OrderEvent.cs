namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Подія, що змінює стан замовлення.
/// </summary>
public enum OrderEvent
{
    /// <summary>Призначити водія.</summary>
    Assign,

    /// <summary>Почати поїздку: пасажир сів у машину.</summary>
    Start,

    /// <summary>Завершити поїздку: пасажира висаджено.</summary>
    Complete,

    /// <summary>Скасувати замовлення.</summary>
    Cancel,
}
