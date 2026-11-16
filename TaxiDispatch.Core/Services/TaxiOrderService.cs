using Microsoft.Extensions.Logging;
using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;

namespace TaxiDispatch.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: сценарій і розрахунок; залежності — інтерфейси.
/// </summary>
public sealed class TaxiOrderService
{
    // Поріг, вище якого вартість одного відрізка вважаємо підозрілою (рівень Warning).
    private const decimal LargeSegmentAmount = 2000m;

    private readonly ITaxiOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<TaxiOrderService> _log;

    /// <summary>
    /// Створює сервіс із впровадженими залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    /// <param name="notifier">Сповіщувач пасажирів.</param>
    /// <param name="log">Журнал сервісу.</param>
    public TaxiOrderService(
        ITaxiOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<TaxiOrderService> log)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>
    /// Приймає замовлення: зберігає його та сповіщає пасажира.
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

        using var scope = _log.BeginScope("Order:{OrderId}", order.Id);
        _repository.Add(order);
        _log.LogInformation("Замовлення прийнято для пасажира {PassengerId}", passenger.Id);
        _notifier.Notify(
            passenger.PhoneNumber,
            $"Замовлення № {order.Id} прийнято, очікуємо маршрут");
    }

    /// <summary>
    /// Додає до замовлення вже перевірений відрізок маршруту (ядро не знає про рядки).
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="segment">Відрізок, отриманий від барикади.</param>
    public void AddSegment(int orderId, RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        using var scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug(
            "Додаємо відрізок до {Destination}: {DistanceKm} км по {PricePerKm} грн",
            segment.Destination,
            segment.DistanceKm,
            segment.PricePerKm);
        TaxiOrder order = OrderOrThrow(orderId);
        if (order.Status != OrderStatus.New)
        {
            throw new DomainRuleException(
                "order.editable",
                $"Редагувати можна лише нове замовлення; замовлення {orderId} — {order.Status}");
        }

        if (segment.Amount > LargeSegmentAmount)
        {
            _log.LogWarning(
                "Незвично дорогий відрізок: {Amount} грн, поріг {Threshold} грн",
                segment.Amount,
                LargeSegmentAmount);
        }

        order.AddSegment(segment);
        _log.LogInformation("Відрізок до {Destination} додано", segment.Destination);
    }

    /// <summary>
    /// Призначає водія замовленню; про порушення правил повідомляє винятком із контекстом.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    public void Assign(int orderId)
    {
        using var scope = _log.BeginScope("Order:{OrderId}", orderId);
        TaxiOrder order = OrderOrThrow(orderId);
        _log.LogDebug("Призначення водія, відрізків {SegmentCount}", order.Segments.Count);
        if (order.Status != OrderStatus.New)
        {
            throw new DomainRuleException(
                "order.assignable",
                $"Стан {order.Status} не дозволяє призначити водія замовленню {orderId}");
        }

        if (order.Segments.Count == 0)
        {
            throw new DomainRuleException(
                "order.route",
                $"Замовлення {orderId} не має жодного відрізка маршруту");
        }

        order.Assign();
        _log.LogInformation("Водія призначено, стан {Status}", order.Status);
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

    /// <summary>
    /// Завантажує замовлення; якщо його немає — виняток із контекстом, а не null.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <returns>Знайдене замовлення.</returns>
    public TaxiOrder Load(int orderId) => OrderOrThrow(orderId);

    /// <summary>
    /// Скасовує замовлення; заборонений перехід перекладає мовою правил предметної області.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="reason">Причина скасування.</param>
    public void Cancel(int orderId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        using var scope = _log.BeginScope("Order:{OrderId}", orderId);
        TaxiOrder order = OrderOrThrow(orderId);
        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            // Виняток — першим аргументом; конкретний тип замість Exception; inner зберігається.
            _log.LogError(
                ex,
                "Скасування відхилено: стан {Status}, причина {Reason}",
                order.Status,
                reason);
            throw new DomainRuleException(
                "order.cancellable",
                $"Замовлення {orderId} у стані {order.Status} скасувати не можна",
                ex);
        }

        _log.LogInformation("Замовлення скасовано: {Reason}", reason);
    }

    private TaxiOrder OrderOrThrow(int orderId) =>
        _repository.GetById(orderId)
            ?? throw new DomainRuleException("order.exists", $"Замовлення {orderId} не знайдено");
}
