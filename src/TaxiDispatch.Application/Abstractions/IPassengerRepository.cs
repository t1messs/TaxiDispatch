using TaxiDispatch.Domain.Passengers;

namespace TaxiDispatch.Application.Abstractions;

/// <summary>
/// Довідник пасажирів; пасажира впізнають за телефоном.
/// </summary>
public interface IPassengerRepository
{
    /// <summary>Шукає пасажира за телефоном.</summary>
    /// <param name="phone">Телефон у форматі +380XXXXXXXXX.</param>
    /// <returns>Пасажир або <c>null</c>, якщо такого телефону немає.</returns>
    Passenger? FindByPhone(string phone);

    /// <summary>Видає номер для нового пасажира.</summary>
    /// <returns>Вільний номер пасажира.</returns>
    int NextId();

    /// <summary>Додає нового пасажира.</summary>
    /// <param name="passenger">Пасажир з унікальним номером і телефоном.</param>
    void Add(Passenger passenger);
}
