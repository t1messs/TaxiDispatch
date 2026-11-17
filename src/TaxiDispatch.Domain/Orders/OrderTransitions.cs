using TaxiDispatch.Domain.Errors;

namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Таблиця переходів станів замовлення (специфікація, таблиця 4.3). Усе, чого немає
/// в таблиці, заборонено.
/// </summary>
public static class OrderTransitions
{
    private static readonly Dictionary<(OrderStatus From, OrderEvent Event), OrderStatus> _allowed =
        new()
        {
            [(OrderStatus.New, OrderEvent.Assign)] = OrderStatus.Assigned,
            [(OrderStatus.New, OrderEvent.Cancel)] = OrderStatus.Cancelled,
            [(OrderStatus.Assigned, OrderEvent.Start)] = OrderStatus.InProgress,
            [(OrderStatus.Assigned, OrderEvent.Cancel)] = OrderStatus.Cancelled,
            [(OrderStatus.InProgress, OrderEvent.Complete)] = OrderStatus.Completed,
        };

    /// <summary>
    /// Перевіряє, що подію дозволено, нічого не змінюючи: сценарій викликає це до того,
    /// як торкнутися інших об'єктів (наприклад, водія).
    /// </summary>
    /// <param name="from">Поточний стан.</param>
    /// <param name="orderEvent">Подія.</param>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public static void EnsureAllowed(OrderStatus from, OrderEvent orderEvent)
    {
        if (!_allowed.ContainsKey((from, orderEvent)))
        {
            throw new InvalidOrderStateException(from, orderEvent);
        }
    }

    /// <summary>
    /// Повертає новий стан за таблицею переходів. Не має побічних ефектів.
    /// </summary>
    /// <param name="from">Поточний стан.</param>
    /// <param name="orderEvent">Подія.</param>
    /// <returns>Стан після події.</returns>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public static OrderStatus Apply(OrderStatus from, OrderEvent orderEvent) =>
        _allowed.TryGetValue((from, orderEvent), out OrderStatus to)
            ? to
            : throw new InvalidOrderStateException(from, orderEvent);
}
