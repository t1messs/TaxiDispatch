using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: сценарій і розрахунок; залежності — інтерфейси.
/// </summary>
public sealed class TaxiOrderService
{
    private readonly ITaxiOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>
    /// Створює сервіс із впровадженими залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    /// <param name="notifier">Сповіщувач пасажирів.</param>
    public TaxiOrderService(
        ITaxiOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    /// <summary>
    /// Приймає замовлення: зберігає його та сповіщає пасажира про суму.
    /// </summary>
    /// <param name="order">Замовлення таксі.</param>
    /// <param name="passenger">Пасажир, якому належить замовлення.</param>
    public void Place(TaxiOrder order, Passenger passenger)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(passenger);
        if (order.PassengerId != passenger.Id)
        {
            throw new ArgumentException("Замовлення належить іншому пасажиру.", nameof(passenger));
        }

        _repository.Add(order);
        _notifier.Notify(
            passenger.PhoneNumber,
            $"Замовлення № {order.Id} прийнято. Сума: {TotalOf(order):0.00}");
    }

    /// <summary>Обчислює суму замовлення за поточною політикою ціни.</summary>
    /// <param name="order">Замовлення таксі.</param>
    /// <returns>Сума всіх відрізків маршруту.</returns>
    public decimal TotalOf(TaxiOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        decimal total = 0m;
        foreach (RouteSegment segment in order.Segments)
        {
            total += _pricing.PriceOf(segment);
        }

        return total;
    }
}
