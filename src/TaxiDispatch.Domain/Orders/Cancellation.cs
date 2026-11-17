namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Скасування замовлення: причина й час.
/// </summary>
public sealed record Cancellation
{
    /// <summary>
    /// Створює запис про скасування.
    /// </summary>
    /// <param name="reason">Причина скасування, не порожня.</param>
    /// <param name="cancelledAt">Час скасування.</param>
    public Cancellation(string reason, DateTimeOffset cancelledAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Reason = reason;
        CancelledAt = cancelledAt;
    }

    /// <summary>Отримує причину скасування.</summary>
    public string Reason { get; }

    /// <summary>Отримує час скасування.</summary>
    public DateTimeOffset CancelledAt { get; }
}
