using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Tests;

// ЛР7, завдання 6: гілки життєвого циклу замовлення, які не покривав жоден тест.
public class TaxiOrderLifecycleTests
{
    [Fact]
    public void FullRide_AssignStartComplete_EndsCompleted()
    {
        // Arrange
        TaxiOrder order = OrderWithRoute(1);

        // Act
        order.Assign();
        order.Start();
        order.Complete();

        // Assert
        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Assign_WithoutRoute_Throws()
    {
        TaxiOrder order = new(2, 100, new DateOnly(2026, 11, 17));

        Assert.Throws<InvalidOperationException>(order.Assign);
    }

    [Fact]
    public void AddSegment_AfterAssign_Throws()
    {
        TaxiOrder order = OrderWithRoute(3);
        order.Assign();

        Assert.Throws<InvalidOperationException>(
            () => order.AddSegment(new RouteSegment("Парк", 1m, 20m)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancel_BeforeRideStarts_BecomesCancelled(bool assigned)
    {
        TaxiOrder order = OrderWithRoute(4);
        if (assigned)
        {
            order.Assign();
        }

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_RideInProgress_Throws()
    {
        TaxiOrder order = OrderWithRoute(5);
        order.Assign();
        order.Start();

        Assert.Throws<InvalidOperationException>(order.Cancel);
        Assert.Equal(OrderStatus.InProgress, order.Status);
    }

    [Fact]
    public void Archive_CompletedOrder_KeepsData()
    {
        TaxiOrder order = OrderWithRoute(6);
        order.Assign();
        order.Start();
        order.Complete();
        var archivedAt = new DateOnly(2026, 11, 18);

        var archived = new ArchivedTaxiOrder(order, archivedAt);

        Assert.Equal(6, archived.Id);
        Assert.Equal(OrderStatus.Completed, archived.Status);
        Assert.Equal(archivedAt, archived.ArchivedAt);
        Assert.Single(archived.Segments);
    }

    [Fact]
    public void Archive_NewOrder_Throws()
    {
        TaxiOrder order = OrderWithRoute(7);

        Assert.Throws<InvalidOperationException>(
            () => new ArchivedTaxiOrder(order, new DateOnly(2026, 11, 18)));
    }

    [Theory]
    [InlineData("0501234567")]
    [InlineData("+38050123456")]
    public void Passenger_WrongPhoneFormat_Throws(string phone)
    {
        var ex = Assert.Throws<ArgumentException>(() => new Passenger(1, "Нікіта", phone));

        Assert.Equal("phoneNumber", ex.ParamName);
    }

    private static TaxiOrder OrderWithRoute(int id)
    {
        TaxiOrder order = new(id, 100, new DateOnly(2026, 11, 17));
        order.AddSegment(new RouteSegment("Вокзал", 4m, 20m));
        return order;
    }
}
