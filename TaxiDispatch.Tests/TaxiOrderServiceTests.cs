using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;
using TaxiDispatch.Core.Services;

namespace TaxiDispatch.Tests;

public class TaxiOrderServiceTests
{
    private static readonly Passenger _passenger = new(100, "Нікіта", "+380501234567");

    [Fact]
    public void TotalOf_TwoSegments_SumsStubPrices()
    {
        // Arrange: стаб повертає 50 грн за будь-який відрізок.
        TaxiOrderService service = CreateService(new FakeTaxiOrderRepository(), price: 50m);
        TaxiOrder order = NewOrder(1);
        order.AddSegment(new RouteSegment("Вокзал", 3m, 20m));
        order.AddSegment(new RouteSegment("Аеропорт", 30m, 15m));

        // Act
        decimal total = service.TotalOf(order);

        // Assert: сервіс лише підсумовує те, що повернула політика ціни.
        Assert.Equal(100m, total);
    }

    [Fact]
    public void Place_ValidOrder_SavesToRepositoryOnce()
    {
        // Arrange
        var repository = new FakeTaxiOrderRepository();
        TaxiOrderService service = CreateService(repository);
        TaxiOrder order = NewOrder(2);

        // Act
        service.Place(order, _passenger);

        // Assert: замовлення збережено рівно один раз і його можна знайти.
        Assert.Equal(1, repository.AddCalls);
        Assert.Same(order, repository.GetById(2));
    }

    [Fact]
    public void Assign_UnknownOrder_ThrowsOrderExists()
    {
        // Arrange: порожній фейк — замовлення 42 не існує.
        var repository = new FakeTaxiOrderRepository();
        TaxiOrderService service = CreateService(repository);

        // Act
        var ex = Assert.Throws<DomainRuleException>(() => service.Assign(42));

        // Assert
        Assert.Equal("order.exists", ex.Rule);
        Assert.Equal(0, repository.AddCalls);
    }

    [Fact]
    public void Assign_PlacedOrderWithRoute_BecomesAssigned()
    {
        // Arrange: фейк зберігає стан між викликами Place → AddSegment → Assign.
        var repository = new FakeTaxiOrderRepository();
        TaxiOrderService service = CreateService(repository);
        service.Place(NewOrder(3), _passenger);
        service.AddSegment(3, new RouteSegment("Центр", 5m, 18m));

        // Act
        service.Assign(3);

        // Assert
        Assert.Equal(OrderStatus.Assigned, service.Load(3).Status);
    }

    private static TaxiOrderService CreateService(
        FakeTaxiOrderRepository repository,
        decimal price = 0m) =>
        new(
            repository,
            new StubPricingPolicy(price),
            new DummyNotifier(),
            NullLogger<TaxiOrderService>.Instance);

    private static TaxiOrder NewOrder(int id) =>
        new(id, _passenger.Id, new DateOnly(2026, 11, 17));
}
