namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Стан водія з погляду диспетчерської.
/// </summary>
public enum DriverStatus
{
    /// <summary>На зміні й вільний: може отримати замовлення.</summary>
    Free,

    /// <summary>Виконує замовлення.</summary>
    Busy,

    /// <summary>Не на зміні: замовлень не отримує.</summary>
    OffShift,
}
