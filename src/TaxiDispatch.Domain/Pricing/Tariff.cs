using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Domain.Pricing;

/// <summary>
/// Тариф класу автомобіля: посадка, ціна за кілометр і мінімальна вартість поїздки, грн.
/// </summary>
public sealed record Tariff
{
    /// <summary>
    /// Створює тариф і перевіряє, що жодна сума не від'ємна.
    /// </summary>
    /// <param name="carClass">Клас автомобіля, до якого застосовується тариф.</param>
    /// <param name="boarding">Плата за посадку, грн.</param>
    /// <param name="perKm">Ціна одного кілометра, грн.</param>
    /// <param name="minimum">Мінімальна вартість поїздки, грн.</param>
    /// <exception cref="ArgumentOutOfRangeException">Сума від'ємна.</exception>
    public Tariff(CarClass carClass, decimal boarding, decimal perKm, decimal minimum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(boarding);
        ArgumentOutOfRangeException.ThrowIfNegative(perKm);
        ArgumentOutOfRangeException.ThrowIfNegative(minimum);

        CarClass = carClass;
        Boarding = boarding;
        PerKm = perKm;
        Minimum = minimum;
    }

    /// <summary>Отримує клас автомобіля.</summary>
    public CarClass CarClass { get; }

    /// <summary>Отримує плату за посадку, грн.</summary>
    public decimal Boarding { get; }

    /// <summary>Отримує ціну одного кілометра, грн.</summary>
    public decimal PerKm { get; }

    /// <summary>Отримує мінімальну вартість поїздки, грн.</summary>
    public decimal Minimum { get; }
}
