using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Domain.Errors;

/// <summary>
/// Подія заборонена для поточного стану замовлення (специфікація, таблиця 4.3).
/// </summary>
public sealed class InvalidOrderStateException : DomainRuleException
{
    /// <summary>
    /// Створює виняток зі станом замовлення та забороненою подією.
    /// </summary>
    /// <param name="status">Поточний стан замовлення.</param>
    /// <param name="attemptedEvent">Подія, яку намагалися застосувати.</param>
    public InvalidOrderStateException(OrderStatus status, OrderEvent attemptedEvent)
        : base(
            "order.transition",
            $"Дію {attemptedEvent} заборонено для замовлення в стані {status}")
    {
        Status = status;
        AttemptedEvent = attemptedEvent;
    }

    /// <summary>Отримує стан замовлення на момент спроби.</summary>
    public OrderStatus Status { get; }

    /// <summary>Отримує заборонену подію.</summary>
    public OrderEvent AttemptedEvent { get; }
}
