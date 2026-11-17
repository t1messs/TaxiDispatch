namespace TaxiDispatch.Domain.Shifts;

/// <summary>
/// Вид зміни диспетчерської.
/// </summary>
public enum ShiftKind
{
    /// <summary>Денна зміна: 08:00–20:00.</summary>
    Day,

    /// <summary>Нічна зміна: 20:00–08:00 наступного дня.</summary>
    Night,
}
