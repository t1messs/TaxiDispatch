using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Application.Orders;
using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Domain.Places;
using TaxiDispatch.Tests.Doubles;

namespace TaxiDispatch.Tests.Application;

// UC-01 «Прийняти замовлення». Годинник — 12:00, тобто денний тариф.
public class OrderIntakeServiceTests
{
    private const string KnownPhone = "+380501112233";

    private static readonly TripPlan _toAirport =
        new(TestData.Station, TestData.Airport, CarClass.Economy);

    private readonly FakeStore _store = new FakeStore().With(new Passenger(1, "Олена", KnownPhone));
    private readonly OrderIntakeService _sut;

    public OrderIntakeServiceTests()
    {
        _sut = new OrderIntakeService(
            _store,
            new TariffFarePolicy(TestData.Tariffs()),
            new FixedTimeProvider(TestData.At(12)),
            NullLogger<OrderIntakeService>.Instance);
    }

    [Fact]
    public void Accept_ValidRequest_CreatesNewOrder()
    {
        Result<AcceptedOrder> result = _sut.Accept(new OrderRequest(KnownPhone, null, _toAirport));

        Assert.True(result.IsSuccess);
        TaxiOrder order = result.Value!.Order;
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(1, order.PassengerId);
        Assert.Same(order, _store.Orders.Find(order.Id));
        Assert.Equal(1, _store.SaveCount);

        // Маршрут 8,590 км: 40 + 12 × 8,59 = 143,08 грн.
        Assert.Equal(143.08m, result.Value.EstimatedFare);
    }

    [Fact]
    public void Accept_UnknownPhone_RegistersPassenger()
    {
        const string newPhone = "+380679998877";

        Result<AcceptedOrder> result = _sut.Accept(new OrderRequest(newPhone, "Тарас", _toAirport));

        Assert.True(result.IsSuccess);
        Assert.Equal("Тарас", result.Value!.Passenger.Name);
        Assert.Equal(result.Value.Passenger, _store.Passengers.FindByPhone(newPhone));
    }

    [Fact]
    public void Accept_UnknownPhoneWithoutName_ReturnsError()
    {
        Result<AcceptedOrder> result =
            _sut.Accept(new OrderRequest("+380679998877", null, _toAirport));

        Assert.False(result.IsSuccess);
        Assert.Empty(_store.Orders.All());
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public void Accept_TooShortTrip_ReturnsError()
    {
        // 0,004° широти = 0,445 км по прямій, менше за 0,5 км.
        var nearby = new Place("P09", "Поруч", TestData.North(0.004));
        var plan = new TripPlan(TestData.Station, nearby, CarClass.Economy);

        Result<AcceptedOrder> result = _sut.Accept(new OrderRequest(KnownPhone, null, plan));

        Assert.False(result.IsSuccess);
        Assert.Empty(_store.Orders.All());
    }
}
