using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: залежності отримує ззовні як інтерфейси.
/// </summary>
public sealed class TaxiOrderService
{
    private readonly ITaxiOrderRepository _repository;
    private readonly IPricingPolicy _pricing;

    /// <summary>
    /// Створює сервіс із впровадженими залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    public TaxiOrderService(ITaxiOrderRepository repository, IPricingPolicy pricing)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    }

    /// <summary>Приймає замовлення до обробки та зберігає його.</summary>
    /// <param name="order">Замовлення таксі.</param>
    public void Place(TaxiOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _repository.Add(order);
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
