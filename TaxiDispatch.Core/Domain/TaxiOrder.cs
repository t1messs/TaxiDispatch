namespace TaxiDispatch.Core.Domain;

/// <summary>Замовлення таксі.</summary>
public sealed class TaxiOrder
{
    private readonly List<RouteSegment> _segments = new();

    public int Id { get; init; }

    public int PassengerId { get; init; }

    public OrderStatus Status { get; set; } = OrderStatus.New;

    public DateTimeOffset CreatedAt { get; init; }

    public IReadOnlyList<RouteSegment> Segments => _segments;

    /// <summary>Додає відрізок маршруту до замовлення.</summary>
    public void AddSegment(RouteSegment segment) => _segments.Add(segment);

    /// <summary>Обчислює загальну вартість поїздки.</summary>
    /// <returns></returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (RouteSegment segment in _segments)
        {
            sum += segment.Amount;
        }

        return sum;
    }
}
