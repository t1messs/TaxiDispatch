using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;
using TaxiDispatch.Core.Services;

namespace TaxiDispatch.Tests;

// ЛР7, завдання 6: гілки правил сервісу (коди DomainRuleException), не покриті раніше.
public class TaxiOrderServiceRulesTests
{
    private static readonly Passenger _passenger = new(100, "Нікіта", "+380501234567");

    private readonly FakeTaxiOrderRepository _repository = new();
    private readonly TaxiOrderService _service;

    public TaxiOrderServiceRulesTests()
    {
        _service = new TaxiOrderService(
            _repository,
            new StubPricingPolicy(0m),
            new DummyNotifier(),
            NullLogger<TaxiOrderService>.Instance);
    }

    [Fact]
    public void Place_OrderOfAnotherPassenger_Throws()
    {
        TaxiOrder order = new(1, 999, new DateOnly(2026, 11, 17));

        var ex = Assert.Throws<ArgumentException>(() => _service.Place(order, _passenger));

        Assert.Equal("passenger", ex.ParamName);
        Assert.Equal(0, _repository.AddCalls);
    }

    [Fact]
    public void AddSegment_ExpensiveSegment_IsStillAdded()
    {
        // Гілка «незвично дорогий відрізок» (> 2000 грн) лише пише Warning у журнал.
        PlaceNew(2);

        _service.AddSegment(2, new RouteSegment("Інше місто", 100m, 25m));

        Assert.Single(_service.Load(2).Segments);
    }

    [Fact]
    public void AddSegment_AssignedOrder_ThrowsOrderEditable()
    {
        PlaceWithRoute(3);
        _service.Assign(3);

        var ex = Assert.Throws<DomainRuleException>(
            () => _service.AddSegment(3, new RouteSegment("Парк", 1m, 20m)));

        Assert.Equal("order.editable", ex.Rule);
    }

    [Fact]
    public void Assign_Twice_ThrowsOrderAssignable()
    {
        PlaceWithRoute(4);
        _service.Assign(4);

        var ex = Assert.Throws<DomainRuleException>(() => _service.Assign(4));

        Assert.Equal("order.assignable", ex.Rule);
    }

    [Fact]
    public void Assign_WithoutRoute_ThrowsOrderRoute()
    {
        PlaceNew(5);

        var ex = Assert.Throws<DomainRuleException>(() => _service.Assign(5));

        Assert.Equal("order.route", ex.Rule);
    }

    [Fact]
    public void Cancel_NewOrder_BecomesCancelled()
    {
        PlaceNew(6);

        _service.Cancel(6, "пасажир передумав");

        Assert.Equal(OrderStatus.Cancelled, _service.Load(6).Status);
    }

    [Fact]
    public void Cancel_RideInProgress_ThrowsOrderCancellableWithInner()
    {
        PlaceWithRoute(7);
        _service.Assign(7);
        _service.Load(7).Start();

        var ex = Assert.Throws<DomainRuleException>(() => _service.Cancel(7, "запізно"));

        Assert.Equal("order.cancellable", ex.Rule);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    private void PlaceNew(int id) =>
        _service.Place(new TaxiOrder(id, _passenger.Id, new DateOnly(2026, 11, 17)), _passenger);

    private void PlaceWithRoute(int id)
    {
        PlaceNew(id);
        _service.AddSegment(id, new RouteSegment("Центр", 5m, 18m));
    }
}
