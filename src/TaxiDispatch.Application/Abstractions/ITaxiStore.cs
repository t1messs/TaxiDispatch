namespace TaxiDispatch.Application.Abstractions;

/// <summary>
/// Дані диспетчерської: довідники й замовлення та збереження змін одним кроком
/// наприкінці сценарію (одиниця роботи).
/// </summary>
public interface ITaxiStore
{
    /// <summary>Отримує сховище замовлень.</summary>
    IOrderRepository Orders { get; }

    /// <summary>Отримує довідник водіїв.</summary>
    IDriverRepository Drivers { get; }

    /// <summary>Отримує довідник пасажирів.</summary>
    IPassengerRepository Passengers { get; }

    /// <summary>Отримує довідник місць.</summary>
    IPlaceDirectory Places { get; }

    /// <summary>Зберігає всі зміни, зроблені сценарієм.</summary>
    void SaveChanges();
}
