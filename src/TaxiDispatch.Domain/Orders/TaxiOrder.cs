namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Замовлення таксі — документ зі станом. Стан змінюють лише методи-події за таблицею
/// <see cref="OrderTransitions"/>; заборонена подія — <c>InvalidOrderStateException</c>.
/// </summary>
public sealed class TaxiOrder
{
    /// <summary>
    /// Створює нове замовлення у стані <see cref="OrderStatus.New"/>.
    /// </summary>
    /// <param name="id">Номер замовлення, більший за нуль.</param>
    /// <param name="passengerId">Номер пасажира, більший за нуль.</param>
    /// <param name="plan">План поїздки.</param>
    /// <param name="createdAt">Час приймання замовлення.</param>
    /// <exception cref="ArgumentException">Номер не додатний або план не передано.</exception>
    public TaxiOrder(int id, int passengerId, TripPlan plan, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(passengerId);
        ArgumentNullException.ThrowIfNull(plan);

        Id = id;
        PassengerId = passengerId;
        Plan = plan;
        CreatedAt = createdAt;
        Status = OrderStatus.New;
    }

    /// <summary>Отримує номер замовлення.</summary>
    public int Id { get; }

    /// <summary>Отримує номер пасажира.</summary>
    public int PassengerId { get; }

    /// <summary>Отримує план поїздки.</summary>
    public TripPlan Plan { get; }

    /// <summary>Отримує час приймання замовлення.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Отримує поточний стан.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Отримує призначення водія; <c>null</c>, поки водія не призначено.</summary>
    public DriverAssignment? Assignment { get; private set; }

    /// <summary>Отримує час посадки; <c>null</c>, поки поїздка не почалася.</summary>
    public DateTimeOffset? StartedAt { get; private set; }

    /// <summary>Отримує підсумок поїздки; <c>null</c>, поки поїздку не завершено.</summary>
    public TripResult? Result { get; private set; }

    /// <summary>Отримує скасування; <c>null</c>, якщо замовлення не скасовано.</summary>
    public Cancellation? Cancellation { get; private set; }

    /// <summary>
    /// Відновлює замовлення зі знімка, прочитаного з файлу.
    /// </summary>
    /// <param name="snapshot">Знімок стану.</param>
    /// <returns>Замовлення в стані зі знімка.</returns>
    /// <exception cref="ArgumentException">Знімок суперечить сам собі.</exception>
    public static TaxiOrder Restore(OrderSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if ((snapshot.Status == OrderStatus.Completed) != (snapshot.Result is not null))
        {
            throw new ArgumentException(
                $"Замовлення {snapshot.Id}: стан і підсумок поїздки не збігаються.",
                nameof(snapshot));
        }

        return new TaxiOrder(snapshot.Id, snapshot.PassengerId, snapshot.Plan, snapshot.CreatedAt)
        {
            Status = snapshot.Status,
            Assignment = snapshot.Assignment,
            StartedAt = snapshot.StartedAt,
            Result = snapshot.Result,
            Cancellation = snapshot.Cancellation,
        };
    }

    /// <summary>Створює знімок поточного стану для збереження.</summary>
    /// <returns>Знімок замовлення.</returns>
    public OrderSnapshot ToSnapshot() => new()
    {
        Id = Id,
        PassengerId = PassengerId,
        Plan = Plan,
        CreatedAt = CreatedAt,
        Status = Status,
        Assignment = Assignment,
        StartedAt = StartedAt,
        Result = Result,
        Cancellation = Cancellation,
    };

    /// <summary>Призначає водія: New → Assigned.</summary>
    /// <param name="assignment">Призначення водія.</param>
    public void Assign(DriverAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        Status = OrderTransitions.Apply(Status, OrderEvent.Assign);
        Assignment = assignment;
    }

    /// <summary>Фіксує посадку пасажира: Assigned → InProgress.</summary>
    /// <param name="startedAt">Час посадки.</param>
    public void Start(DateTimeOffset startedAt)
    {
        Status = OrderTransitions.Apply(Status, OrderEvent.Start);
        StartedAt = startedAt;
    }

    /// <summary>Завершує поїздку: InProgress → Completed.</summary>
    /// <param name="result">Пробіг, вартість і час завершення.</param>
    public void Complete(TripResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Status = OrderTransitions.Apply(Status, OrderEvent.Complete);
        Result = result;
    }

    /// <summary>Скасовує замовлення: New або Assigned → Cancelled.</summary>
    /// <param name="cancellation">Причина й час скасування.</param>
    public void Cancel(Cancellation cancellation)
    {
        ArgumentNullException.ThrowIfNull(cancellation);
        Status = OrderTransitions.Apply(Status, OrderEvent.Cancel);
        Cancellation = cancellation;
    }
}
