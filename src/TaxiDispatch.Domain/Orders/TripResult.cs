namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Підсумок виконаної поїздки: пробіг, вартість і час завершення.
/// </summary>
public sealed record TripResult
{
    /// <summary>
    /// Створює підсумок поїздки.
    /// </summary>
    /// <param name="actualKm">Фактичний пробіг, км, не від'ємний.</param>
    /// <param name="fare">Вартість поїздки, грн, не від'ємна.</param>
    /// <param name="completedAt">Час завершення.</param>
    public TripResult(decimal actualKm, decimal fare, DateTimeOffset completedAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(actualKm);
        ArgumentOutOfRangeException.ThrowIfNegative(fare);

        ActualKm = actualKm;
        Fare = fare;
        CompletedAt = completedAt;
    }

    /// <summary>Отримує фактичний пробіг, км.</summary>
    public decimal ActualKm { get; }

    /// <summary>Отримує вартість поїздки, грн.</summary>
    public decimal Fare { get; }

    /// <summary>Отримує час завершення.</summary>
    public DateTimeOffset CompletedAt { get; }
}
