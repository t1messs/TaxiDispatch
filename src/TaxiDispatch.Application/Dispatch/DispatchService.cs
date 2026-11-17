using Microsoft.Extensions.Logging;
using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Errors;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Application.Dispatch;

/// <summary>
/// Сценарії зміни стану замовлення: UC-02 «Призначити водія», UC-03 «Виконати поїздку»,
/// UC-04 «Скасувати замовлення».
/// </summary>
public sealed class DispatchService
{
    private readonly ITaxiStore _store;
    private readonly IFarePolicy _fares;
    private readonly TimeProvider _clock;
    private readonly ILogger<DispatchService> _log;

    /// <summary>
    /// Створює сервіс із впровадженими залежностями.
    /// </summary>
    /// <param name="store">Дані диспетчерської.</param>
    /// <param name="fares">Політика вартості.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <param name="log">Журнал сценаріїв.</param>
    public DispatchService(
        ITaxiStore store,
        IFarePolicy fares,
        TimeProvider clock,
        ILogger<DispatchService> log)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _fares = fares ?? throw new ArgumentNullException(nameof(fares));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>
    /// UC-02: призначає найближчого вільного водія потрібного класу (правило 4.2).
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <returns>Обраний водій або пояснення, чому призначення не відбулося.</returns>
    /// <exception cref="InvalidOrderStateException">Замовлення не в стані New.</exception>
    public Result<DriverMatch> Assign(int orderId) => Run("UC-02", orderId, order =>
    {
        OrderTransitions.EnsureAllowed(order.Status, OrderEvent.Assign);
        DriverMatch? match = DriverMatcher.Choose(
            _store.Drivers.All(),
            order.Plan.Pickup.Location,
            order.Plan.CarClass);
        if (match is null)
        {
            _log.LogWarning(
                "Відхилено: немає вільного водія класу {CarClass}",
                order.Plan.CarClass);
            return Result.Fail<DriverMatch>(
                $"Немає вільного водія класу {order.Plan.CarClass} ближче ніж "
                + $"{DriverMatcher.MaxPickupKm} км. Спробуйте пізніше.");
        }

        order.Assign(new DriverAssignment(match.Driver.Id, match.PickupKm, _clock.GetLocalNow()));
        match.Driver.TakeOrder();
        return Result.Ok(match);
    });

    /// <summary>UC-03, крок 2: фіксує посадку пасажира.</summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <returns>Замовлення в стані InProgress або пояснення помилки.</returns>
    /// <exception cref="InvalidOrderStateException">Замовлення не в стані Assigned.</exception>
    public Result<TaxiOrder> Start(int orderId) => Run("UC-03", orderId, order =>
    {
        order.Start(_clock.GetLocalNow());
        return Result.Ok(order);
    });

    /// <summary>
    /// UC-03, кроки 3–5: рахує вартість, завершує поїздку й звільняє водія в точці призначення.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="actualKm">Фактичний пробіг, км; <c>null</c> — як довжина маршруту.</param>
    /// <returns>Підсумок поїздки або пояснення помилки.</returns>
    /// <exception cref="InvalidOrderStateException">Замовлення не в стані InProgress.</exception>
    public Result<TripResult> Complete(int orderId, decimal? actualKm) =>
        Run("UC-03", orderId, order => CompleteTrip(order, actualKm));

    /// <summary>UC-04: скасовує замовлення й звільняє призначеного водія на місці.</summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="reason">Причина, перевірена барикадою.</param>
    /// <returns>Скасоване замовлення або пояснення помилки.</returns>
    /// <exception cref="InvalidOrderStateException">Поїздка почалася або завершилася.</exception>
    public Result<TaxiOrder> Cancel(int orderId, string reason) => Run("UC-04", orderId, order =>
    {
        order.Cancel(new Cancellation(reason, _clock.GetLocalNow()));
        if (order.Assignment is not null)
        {
            Driver driver = AssignedDriver(order);
            driver.Release(driver.Location);
        }

        return Result.Ok(order);
    });

    private Result<TripResult> CompleteTrip(TaxiOrder order, decimal? actualKm)
    {
        OrderTransitions.EnsureAllowed(order.Status, OrderEvent.Complete);
        decimal km = actualKm ?? order.Plan.RouteKm;
        DateTimeOffset pickupTime = order.StartedAt ?? order.CreatedAt;
        var result = new TripResult(
            km,
            _fares.Quote(order.Plan.CarClass, km, pickupTime),
            _clock.GetLocalNow());
        order.Complete(result);
        AssignedDriver(order).Release(order.Plan.Dropoff.Location);
        return Result.Ok(result);
    }

    private Driver AssignedDriver(TaxiOrder order)
    {
        int driverId = order.Assignment?.DriverId
            ?? throw new InvalidOperationException($"Замовлення {order.Id} не має водія.");
        return _store.Drivers.Find(driverId)
            ?? throw new InvalidOperationException($"Водія {driverId} немає в довіднику.");
    }

    // Спільний каркас сценаріїв: область журналу, пошук замовлення, збереження лише після
    // успіху; порушення правила домену пишеться як Warning і летить далі до консолі.
    private Result<T> Run<T>(string useCase, int orderId, Func<TaxiOrder, Result<T>> action)
    {
        using IDisposable? scope = _log.BeginScope(useCase);
        _log.LogInformation("Початок: замовлення {OrderId}", orderId);
        TaxiOrder? order = _store.Orders.Find(orderId);
        if (order is null)
        {
            _log.LogWarning("Відхилено: замовлення {OrderId} не знайдено", orderId);
            return Result.Fail<T>($"Замовлення № {orderId} не знайдено.");
        }

        try
        {
            Result<T> result = action(order);
            if (result.IsSuccess)
            {
                _store.SaveChanges();
                _log.LogInformation(
                    "Завершено: замовлення {OrderId}, стан {Status}",
                    orderId,
                    order.Status);
            }

            return result;
        }
        catch (DomainRuleException error)
        {
            _log.LogWarning("Відхилено: замовлення {OrderId}, {Reason}", orderId, error.Message);
            throw;
        }
    }
}
