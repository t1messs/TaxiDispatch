namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Замовлення таксі — агрегат: стан змінюють лише методи-переходи.
/// </summary>
public sealed class TaxiOrder
{
    private readonly List<RouteSegment> _segments = new();

    /// <summary>
    /// Створює нове замовлення у стані <see cref="OrderStatus.New"/>.
    /// </summary>
    /// <param name="id">Ідентифікатор замовлення, більший за нуль.</param>
    /// <param name="passengerId">Ідентифікатор пасажира, більший за нуль.</param>
    /// <param name="createdAt">Дата створення замовлення.</param>
    public TaxiOrder(int id, int passengerId, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(passengerId);

        Id = id;
        PassengerId = passengerId;
        CreatedAt = createdAt;
        Status = OrderStatus.New;
    }

    /// <summary>Отримує ідентифікатор замовлення.</summary>
    public int Id { get; }

    /// <summary>Отримує ідентифікатор пасажира.</summary>
    public int PassengerId { get; }

    /// <summary>Отримує дату створення замовлення.</summary>
    public DateOnly CreatedAt { get; }

    /// <summary>Отримує поточний стан; змінюється лише методами-переходами.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Отримує відрізки маршруту лише для читання.</summary>
    public IReadOnlyList<RouteSegment> Segments => _segments.AsReadOnly();

    /// <summary>
    /// Додає відрізок маршруту; дозволено лише для нового замовлення.
    /// </summary>
    /// <param name="segment">Відрізок маршруту.</param>
    public void AddSegment(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        if (Status != OrderStatus.New)
        {
            throw new InvalidOperationException("Відрізки додають лише до нового замовлення.");
        }

        _segments.Add(segment);
    }

    /// <summary>
    /// Призначає водія: New → Assigned, якщо є хоча б один відрізок.
    /// </summary>
    public void Assign()
    {
        EnsureStatus(OrderStatus.New);
        if (_segments.Count == 0)
        {
            throw new InvalidOperationException("Замовлення без маршруту не можна призначити.");
        }

        Status = OrderStatus.Assigned;
    }

    /// <summary>
    /// Починає поїздку: Assigned → InProgress.
    /// </summary>
    public void Start()
    {
        EnsureStatus(OrderStatus.Assigned);
        Status = OrderStatus.InProgress;
    }

    /// <summary>
    /// Завершує поїздку: InProgress → Completed.
    /// </summary>
    public void Complete()
    {
        EnsureStatus(OrderStatus.InProgress);
        Status = OrderStatus.Completed;
    }

    /// <summary>
    /// Скасовує замовлення: дозволено зі станів New і Assigned.
    /// </summary>
    public void Cancel()
    {
        if (Status != OrderStatus.New && Status != OrderStatus.Assigned)
        {
            throw new InvalidOperationException("Скасувати можна лише до початку поїздки.");
        }

        Status = OrderStatus.Cancelled;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException("Перехід із поточного стану заборонено.");
        }
    }
}
