using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Domain.Pricing;

/// <summary>
/// Тарифна сітка: по одному тарифу на кожен клас автомобіля.
/// </summary>
public sealed class TariffBook
{
    private readonly Dictionary<CarClass, Tariff> _tariffs;

    /// <summary>
    /// Створює тарифну сітку й перевіряє, що кожен клас має рівно один тариф.
    /// </summary>
    /// <param name="tariffs">Тарифи, наприклад прочитані з <c>data/tariffs.json</c>.</param>
    /// <exception cref="ArgumentException">Класу бракує тарифу або тариф повторюється.</exception>
    public TariffBook(IEnumerable<Tariff> tariffs)
    {
        ArgumentNullException.ThrowIfNull(tariffs);
        _tariffs = tariffs.ToDictionary(tariff => tariff.CarClass);
        foreach (CarClass carClass in Enum.GetValues<CarClass>())
        {
            if (!_tariffs.ContainsKey(carClass))
            {
                throw new ArgumentException($"Немає тарифу для класу {carClass}", nameof(tariffs));
            }
        }
    }

    /// <summary>Повертає тариф класу автомобіля.</summary>
    /// <param name="carClass">Клас автомобіля.</param>
    /// <returns>Тариф цього класу.</returns>
    public Tariff For(CarClass carClass) => _tariffs[carClass];
}
