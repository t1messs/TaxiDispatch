using Microsoft.Extensions.Logging;
using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Сценарій UC-05 «Знайти замовлення» за станом, датою й телефоном пасажира.
/// </summary>
public sealed class OrderQueryService
{
    private readonly ITaxiStore _store;
    private readonly ILogger<OrderQueryService> _log;

    /// <summary>
    /// Створює сервіс пошуку.
    /// </summary>
    /// <param name="store">Дані диспетчерської.</param>
    /// <param name="log">Журнал сценарію.</param>
    public OrderQueryService(ITaxiStore store, ILogger<OrderQueryService> log)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>
    /// Знаходить замовлення, що відповідають усім заданим критеріям, за часом створення.
    /// </summary>
    /// <param name="criteria">Критерії пошуку; хоча б один має бути заданий.</param>
    /// <returns>Знайдені замовлення (можливо, жодного) або пояснення помилки.</returns>
    public Result<IReadOnlyList<TaxiOrder>> Find(OrderSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        using IDisposable? scope = _log.BeginScope("UC-05");
        if (criteria.IsEmpty)
        {
            _log.LogWarning("Відхилено: пошук без критеріїв");
            return Result.Fail<IReadOnlyList<TaxiOrder>>(
                "Задайте хоча б один критерій: стан, дату або телефон.");
        }

        // Невідомий телефон — номер 0, якого немає в жодного пасажира: результат порожній.
        int? passengerId = criteria.Phone is null
            ? null
            : _store.Passengers.FindByPhone(criteria.Phone)?.Id ?? 0;
        List<TaxiOrder> found = _store.Orders.All()
            .Where(order => criteria.Status is null || order.Status == criteria.Status)
            .Where(order => criteria.CreatedOn is null
                || DateOnly.FromDateTime(order.CreatedAt.DateTime) == criteria.CreatedOn)
            .Where(order => passengerId is null || order.PassengerId == passengerId)
            .OrderBy(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .ToList();
        _log.LogInformation("Завершено: знайдено {Count}", found.Count);
        return Result.Ok<IReadOnlyList<TaxiOrder>>(found);
    }
}
