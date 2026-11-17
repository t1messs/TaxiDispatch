using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Application.Pricing;

/// <summary>
/// Політика вартості поїздки — точка розширення: новий спосіб розрахунку (наприклад,
/// коефіцієнт попиту) додається новою реалізацією без зміни сценаріїв.
/// </summary>
public interface IFarePolicy
{
    /// <summary>Обчислює вартість поїздки.</summary>
    /// <param name="carClass">Клас автомобіля.</param>
    /// <param name="distanceKm">Пробіг, км; не від'ємний.</param>
    /// <param name="pickupTime">Час посадки (для попередньої вартості — час розрахунку).</param>
    /// <returns>Вартість, грн, округлена до копійок.</returns>
    decimal Quote(CarClass carClass, decimal distanceKm, DateTimeOffset pickupTime);
}
