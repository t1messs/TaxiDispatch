using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;

namespace TaxiDispatch.Application.Orders;

/// <summary>
/// Результат UC-01: прийняте замовлення, його пасажир і попередня вартість.
/// </summary>
public sealed record AcceptedOrder
{
    /// <summary>
    /// Створює результат приймання.
    /// </summary>
    /// <param name="order">Нове замовлення.</param>
    /// <param name="passenger">Пасажир замовлення.</param>
    /// <param name="estimatedFare">Попередня вартість, грн.</param>
    public AcceptedOrder(TaxiOrder order, Passenger passenger, decimal estimatedFare)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(passenger);

        Order = order;
        Passenger = passenger;
        EstimatedFare = estimatedFare;
    }

    /// <summary>Отримує нове замовлення.</summary>
    public TaxiOrder Order { get; }

    /// <summary>Отримує пасажира.</summary>
    public Passenger Passenger { get; }

    /// <summary>Отримує попередню вартість, грн.</summary>
    public decimal EstimatedFare { get; }
}
