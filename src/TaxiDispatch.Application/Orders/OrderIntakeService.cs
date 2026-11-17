using Microsoft.Extensions.Logging;
using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Geo;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;

namespace TaxiDispatch.Application.Orders;

/// <summary>
/// Сценарій UC-01 «Прийняти замовлення»: реєструє пасажира за потреби, створює замовлення
/// й рахує попередню вартість.
/// </summary>
public sealed class OrderIntakeService
{
    /// <summary>Найменша відстань між подачею й призначенням по прямій, км.</summary>
    public const decimal MinTripKm = 0.5m;

    private readonly ITaxiStore _store;
    private readonly IFarePolicy _fares;
    private readonly TimeProvider _clock;
    private readonly ILogger<OrderIntakeService> _log;

    /// <summary>
    /// Створює сервіс із впровадженими залежностями.
    /// </summary>
    /// <param name="store">Дані диспетчерської.</param>
    /// <param name="fares">Політика вартості.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <param name="log">Журнал сценарію.</param>
    public OrderIntakeService(
        ITaxiStore store,
        IFarePolicy fares,
        TimeProvider clock,
        ILogger<OrderIntakeService> log)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _fares = fares ?? throw new ArgumentNullException(nameof(fares));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>Шукає пасажира за телефоном (крок 2 сценарію).</summary>
    /// <param name="phone">Телефон, перевірений барикадою.</param>
    /// <returns>Пасажир або <c>null</c>, якщо телефон новий.</returns>
    public Passenger? FindPassenger(string phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        return _store.Passengers.FindByPhone(phone);
    }

    /// <summary>
    /// Приймає замовлення: перевіряє довжину поїздки, за потреби реєструє пасажира,
    /// зберігає нове замовлення й рахує попередню вартість.
    /// </summary>
    /// <param name="request">Перевірені дані замовлення.</param>
    /// <returns>Прийняте замовлення або пояснення, чому його відхилено.</returns>
    public Result<AcceptedOrder> Accept(OrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        using IDisposable? scope = _log.BeginScope("UC-01");
        TripPlan plan = request.Plan;
        _log.LogInformation(
            "Початок: {Pickup} → {Dropoff}, клас {CarClass}",
            plan.Pickup.Code,
            plan.Dropoff.Code,
            plan.CarClass);
        if (GeoDistance.KilometersBetween(plan.Pickup.Location, plan.Dropoff.Location) < MinTripKm)
        {
            _log.LogWarning("Відхилено: подача й призначення ближчі за {MinTripKm} км", MinTripKm);
            return Result.Fail<AcceptedOrder>(
                $"Подача й призначення ближчі за {MinTripKm} км — замовлення не прийнято.");
        }

        Passenger? passenger = _store.Passengers.FindByPhone(request.Phone) ?? Register(request);
        if (passenger is null)
        {
            _log.LogWarning("Відхилено: новий пасажир без імені");
            return Result.Fail<AcceptedOrder>("Пасажира з таким телефоном немає — потрібне ім'я.");
        }

        DateTimeOffset now = _clock.GetLocalNow();
        var order = new TaxiOrder(_store.Orders.NextId(), passenger.Id, plan, now);
        _store.Orders.Add(order);
        _store.SaveChanges();
        decimal estimate = _fares.Quote(plan.CarClass, plan.RouteKm, now);
        _log.LogInformation(
            "Завершено: замовлення {OrderId}, оцінка {Estimate} грн",
            order.Id,
            estimate);
        return Result.Ok(new AcceptedOrder(order, passenger, estimate));
    }

    private Passenger? Register(OrderRequest request)
    {
        if (request.PassengerName is null)
        {
            return null;
        }

        int id = _store.Passengers.NextId();
        var passenger = new Passenger(id, request.PassengerName, request.Phone);
        _store.Passengers.Add(passenger);
        _log.LogInformation("Зареєстровано пасажира {PassengerId}", passenger.Id);
        return passenger;
    }
}
