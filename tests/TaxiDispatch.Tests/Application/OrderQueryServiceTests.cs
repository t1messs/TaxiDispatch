using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Application.Queries;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Tests.Doubles;

namespace TaxiDispatch.Tests.Application;

// UC-05 «Знайти замовлення».
public class OrderQueryServiceTests
{
    private const string OlenaPhone = "+380501112233";

    private static readonly TripPlan _plan =
        new(TestData.Station, TestData.Airport, CarClass.Economy);

    private readonly FakeStore _store = new();
    private readonly OrderQueryService _sut;

    public OrderQueryServiceTests()
    {
        // 17.11: № 1 (Олена, 10:00) і № 2 (Тарас, 09:00) нові; 18.11: № 3 (Олена) скасоване.
        TaxiOrder cancelled = new(3, 1, _plan, TestData.At(11, day: 18));
        cancelled.Cancel(new Cancellation("передумала", TestData.At(12, day: 18)));
        _store
            .With(new Passenger(1, "Олена", OlenaPhone), new Passenger(2, "Тарас", "+380679998877"))
            .With(
                new TaxiOrder(1, 1, _plan, TestData.At(10)),
                new TaxiOrder(2, 2, _plan, TestData.At(9)),
                cancelled);
        _sut = new OrderQueryService(_store, NullLogger<OrderQueryService>.Instance);
    }

    [Fact]
    public void Find_NoCriteria_ReturnsError()
    {
        Result<IReadOnlyList<TaxiOrder>> result = _sut.Find(new OrderSearchCriteria());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Find_ByStatusAndDate_ReturnsSortedMatches()
    {
        var criteria = new OrderSearchCriteria
        {
            Status = OrderStatus.New,
            CreatedOn = new DateOnly(2026, 11, 17),
        };

        Result<IReadOnlyList<TaxiOrder>> result = _sut.Find(criteria);

        int[] byCreationTime = [2, 1];
        Assert.Equal(byCreationTime, result.Value!.Select(order => order.Id));
    }

    [Fact]
    public void Find_NothingMatches_ReturnsEmpty()
    {
        var criteria = new OrderSearchCriteria { Status = OrderStatus.Completed };

        Result<IReadOnlyList<TaxiOrder>> result = _sut.Find(criteria);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void Find_ByPhone_ReturnsOnlyThisPassengerOrders()
    {
        Result<IReadOnlyList<TaxiOrder>> result =
            _sut.Find(new OrderSearchCriteria { Phone = OlenaPhone });

        int[] olenaOrders = [1, 3];
        Assert.Equal(olenaOrders, result.Value!.Select(order => order.Id));
    }

    [Fact]
    public void Find_UnknownPhone_ReturnsEmpty()
    {
        Result<IReadOnlyList<TaxiOrder>> result =
            _sut.Find(new OrderSearchCriteria { Phone = "+380630000000" });

        Assert.Empty(result.Value!);
    }
}
