namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Стан замовлення; переходи між станами — у <see cref="OrderTransitions"/>.
/// </summary>
public enum OrderStatus
{
    /// <summary>Прийняте, водія ще не призначено.</summary>
    New,

    /// <summary>Водія призначено, він їде до пасажира.</summary>
    Assigned,

    /// <summary>Пасажир у машині, поїздка триває.</summary>
    InProgress,

    /// <summary>Поїздку завершено, вартість визначено.</summary>
    Completed,

    /// <summary>Замовлення скасовано до початку поїздки.</summary>
    Cancelled,
}
