using TaxiDispatch.Domain.Errors;
using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Tests.Domain;

// Таблиця переходів 4.3: дозволені переходи й заборонені події.
public class OrderTransitionsTests
{
    [Theory]
    [InlineData(OrderStatus.New, OrderEvent.Assign, OrderStatus.Assigned)]
    [InlineData(OrderStatus.New, OrderEvent.Cancel, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Assigned, OrderEvent.Start, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Assigned, OrderEvent.Cancel, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.InProgress, OrderEvent.Complete, OrderStatus.Completed)]
    public void Apply_AllowedTransition_ReturnsNewStatus(
        OrderStatus from,
        OrderEvent orderEvent,
        OrderStatus expected)
    {
        OrderStatus to = OrderTransitions.Apply(from, orderEvent);

        Assert.Equal(expected, to);
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderEvent.Start)]
    [InlineData(OrderStatus.New, OrderEvent.Complete)]
    [InlineData(OrderStatus.Assigned, OrderEvent.Assign)]
    [InlineData(OrderStatus.InProgress, OrderEvent.Cancel)]
    [InlineData(OrderStatus.Completed, OrderEvent.Cancel)]
    [InlineData(OrderStatus.Cancelled, OrderEvent.Assign)]
    public void Apply_ForbiddenTransition_Throws(OrderStatus from, OrderEvent orderEvent)
    {
        var error = Assert.Throws<InvalidOrderStateException>(
            () => OrderTransitions.Apply(from, orderEvent));

        Assert.Equal(from, error.Status);
        Assert.Equal(orderEvent, error.AttemptedEvent);
    }
}
