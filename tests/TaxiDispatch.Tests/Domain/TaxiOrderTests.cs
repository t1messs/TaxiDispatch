using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.Tests.Domain;

public class TaxiOrderTests
{
    [Fact]
    public void Complete_AfterAssignAndStart_StoresResult()
    {
        TaxiOrder order = TestData.NewOrder();

        order.Assign(new DriverAssignment(7, 1.112m, TestData.At(12)));
        order.Start(TestData.At(13));
        order.Complete(new TripResult(8.5m, 142m, TestData.At(14)));

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(7, order.Assignment!.DriverId);
        Assert.Equal(142m, order.Result!.Fare);
    }

    [Fact]
    public void Restore_SnapshotOfCompletedOrder_KeepsStateAndResult()
    {
        TaxiOrder order = TestData.NewOrder();
        order.Assign(new DriverAssignment(7, 1.112m, TestData.At(12)));
        order.Start(TestData.At(13));
        order.Complete(new TripResult(8.5m, 142m, TestData.At(14)));

        TaxiOrder restored = TaxiOrder.Restore(order.ToSnapshot());

        Assert.Equal(order.ToSnapshot(), restored.ToSnapshot());
    }

    [Fact]
    public void Restore_CompletedWithoutResult_Throws()
    {
        OrderSnapshot broken = TestData.NewOrder().ToSnapshot() with
        {
            Status = OrderStatus.Completed,
        };

        Assert.Throws<ArgumentException>(() => TaxiOrder.Restore(broken));
    }

    [Fact]
    public void RouteKm_StraightLineOneKilometre_MultipliesByRoadFactor()
    {
        var north = new Place("P03", "Північ", TestData.North(0.01));
        var plan = new TripPlan(TestData.Station, north, CarClass.Economy);

        // 1,112 км по прямій × 1,3 = 1,4456 → 1,446 км.
        Assert.Equal(1.446m, plan.RouteKm);
    }
}
