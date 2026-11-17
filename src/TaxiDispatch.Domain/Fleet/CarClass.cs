namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Клас автомобіля: визначає тариф замовлення й те, які водії можуть його виконати.
/// </summary>
public enum CarClass
{
    /// <summary>Економ: найнижчий тариф.</summary>
    Economy,

    /// <summary>Комфорт: просторіший салон, середній тариф.</summary>
    Comfort,

    /// <summary>Бізнес: представницький автомобіль, найвищий тариф.</summary>
    Business,
}
