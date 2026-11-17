using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Application.Dispatch;
using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Errors;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Tests.Doubles;

namespace TaxiDispatch.Tests.Application;

// UC-02 «Призначити водія», UC-03 «Виконати поїздку», UC-04 «Скасувати замовлення».
// Замовлення «Вокзал → Аеропорт» економ-класу; подача — TestData.Center.
public class DispatchServiceTests
{
    private readonly FakeStore _store = new();
    private readonly FixedTimeProvider _clock = new(TestData.At(12));
    private readonly DispatchService _sut;

    public DispatchServiceTests()
    {
        _sut = new DispatchService(
            _store,
            new TariffFarePolicy(TestData.Tariffs()),
            _clock,
            NullLogger<DispatchService>.Instance);
    }

    [Fact]
    public void Assign_FreeDriverNearby_AssignsNearest()
    {
        Driver near = TestData.FreeDriver(1, TestData.North(0.01));
        Driver far = TestData.FreeDriver(2, TestData.North(0.02));
        TaxiOrder order = AddOrder(far, near);

        Result<DriverMatch> result = _sut.Assign(order.Id);

        Assert.True(result.IsSuccess);
        Assert.Same(near, result.Value!.Driver);
        Assert.Equal(OrderStatus.Assigned, order.Status);
        Assert.Equal(1.112m, order.Assignment!.PickupKm);
        Assert.Equal(DriverStatus.Busy, near.Status);
        Assert.Equal(DriverStatus.Free, far.Status);
        Assert.Equal(1, _store.SaveCount);
    }

    [Fact]
    public void Assign_NoDriverInRadius_ReturnsFailureAndKeepsNew()
    {
        // 0,1° широти = 11,1 км: далі за радіус 5 км.
        TaxiOrder order = AddOrder(TestData.FreeDriver(1, TestData.North(0.1)));

        Result<DriverMatch> result = _sut.Assign(order.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public void Assign_OrderNotNew_ThrowsAndKeepsOtherDriverFree()
    {
        Driver spare = TestData.FreeDriver(2, TestData.North(0.02));
        TaxiOrder order = AddOrder(TestData.FreeDriver(1, TestData.North(0.01)), spare);
        _sut.Assign(order.Id);

        Assert.Throws<InvalidOrderStateException>(() => _sut.Assign(order.Id));
        Assert.Equal(DriverStatus.Free, spare.Status);
    }

    [Fact]
    public void Assign_UnknownOrder_ReturnsError()
    {
        Result<DriverMatch> result = _sut.Assign(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("99", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Start_OrderNotAssigned_Throws()
    {
        TaxiOrder order = AddOrder();

        var error = Assert.Throws<InvalidOrderStateException>(() => _sut.Start(order.Id));

        Assert.Equal(OrderEvent.Start, error.AttemptedEvent);
        Assert.Equal(OrderStatus.New, order.Status);
    }

    [Fact]
    public void Complete_InProgressOrder_SetsFareAndFreesDriver()
    {
        Driver driver = TestData.FreeDriver(1, TestData.North(0.01));
        TaxiOrder order = AddOrder(driver);
        _sut.Assign(order.Id);
        _clock.Set(TestData.At(13));
        _sut.Start(order.Id);
        _clock.Set(TestData.At(14));

        Result<TripResult> result = _sut.Complete(order.Id, 10m);

        // День, економ, 10 км: 40 + 12 × 10 = 160 грн.
        Assert.Equal(160m, result.Value!.Fare);
        Assert.Equal(TestData.At(14), result.Value.CompletedAt);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(DriverStatus.Free, driver.Status);
        Assert.Equal(TestData.Airport.Location, driver.Location);
    }

    [Fact]
    public void Complete_NoActualKmAndNightPickup_UsesRouteAndSurcharge()
    {
        TaxiOrder order = AddOrder(TestData.FreeDriver(1, TestData.North(0.01)));
        _sut.Assign(order.Id);
        _clock.Set(TestData.At(23));
        _sut.Start(order.Id);

        Result<TripResult> result = _sut.Complete(order.Id, null);

        // Маршрут 8,590 км: (40 + 12 × 8,59) × 1,25 = 178,85 грн.
        Assert.Equal(8.590m, result.Value!.ActualKm);
        Assert.Equal(178.85m, result.Value.Fare);
    }

    [Fact]
    public void Cancel_AssignedOrder_FreesDriver()
    {
        Driver driver = TestData.FreeDriver(1, TestData.North(0.01));
        TaxiOrder order = AddOrder(driver);
        _sut.Assign(order.Id);

        Result<TaxiOrder> result = _sut.Cancel(order.Id, "пасажир передумав");

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("пасажир передумав", order.Cancellation!.Reason);
        Assert.Equal(DriverStatus.Free, driver.Status);
    }

    [Fact]
    public void Cancel_InProgressOrder_Throws()
    {
        TaxiOrder order = AddOrder(TestData.FreeDriver(1, TestData.North(0.01)));
        _sut.Assign(order.Id);
        _sut.Start(order.Id);

        Assert.Throws<InvalidOrderStateException>(() => _sut.Cancel(order.Id, "пізно"));
        Assert.Equal(OrderStatus.InProgress, order.Status);
    }

    private TaxiOrder AddOrder(params Driver[] drivers)
    {
        TaxiOrder order = TestData.NewOrder();
        _store.With(drivers).With(order);
        return order;
    }
}
