namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Архівне замовлення не є замовленням: воно містить його і лише показує дані.
/// </summary>
public sealed class ArchivedTaxiOrder
{
    private readonly TaxiOrder _order;

    /// <summary>
    /// Поміщає завершене або скасоване замовлення в архів.
    /// </summary>
    /// <param name="order">Замовлення у стані Completed або Cancelled.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    public ArchivedTaxiOrder(TaxiOrder order, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "В архів потрапляють лише завершені або скасовані замовлення.");
        }

        _order = order;
        ArchivedAt = archivedAt;
    }

    /// <summary>Отримує дату архівування.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Отримує ідентифікатор замовлення.</summary>
    public int Id => _order.Id;

    /// <summary>Отримує кінцевий стан замовлення.</summary>
    public OrderStatus Status => _order.Status;

    /// <summary>Отримує відрізки маршруту лише для читання.</summary>
    public IReadOnlyList<RouteSegment> Segments => _order.Segments;

    // Методу AddSegment немає взагалі: порушити контракт нічим.
}
