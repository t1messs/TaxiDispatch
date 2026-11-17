using Microsoft.Extensions.Logging.Abstractions;
using TaxiDispatch.Application.Queries;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Shifts;
using TaxiDispatch.Tests.Doubles;

namespace TaxiDispatch.Tests.Application;

// UC-06 «Звіт „Поїздки за зміну“». Зміна — денна 17.11.2026, 08:00–20:00.
public class ShiftReportServiceTests
{
    private static readonly Shift _dayShift = new(new DateOnly(2026, 11, 17), ShiftKind.Day);

    private readonly FakeStore _store = new FakeStore().With(
        TestData.FreeDriver(1, TestData.Center),
        TestData.FreeDriver(2, TestData.Center));

    private ShiftReportService Sut => new(_store, NullLogger<ShiftReportService>.Instance);

    [Fact]
    public void Build_EmptyShift_ReturnsZeroTotals()
    {
        ShiftReport report = Sut.Build(_dayShift);

        Assert.Empty(report.Rows);
        Assert.Equal(0m, report.Revenue);
        Assert.Equal(0m, report.AverageFare);
        Assert.Equal(0, report.CancelledCount);
    }

    [Fact]
    public void Build_OneRide_ReturnsOneRow()
    {
        _store.With(Completed(1, driverId: 1, TestData.At(15), 160m));

        ShiftReport report = Sut.Build(_dayShift);

        ShiftReportRow row = Assert.Single(report.Rows);
        Assert.Equal("Водій 1", row.DriverName);
        Assert.Equal(160m, row.Fare);
        Assert.Equal(160m, report.AverageFare);
    }

    [Fact]
    public void Build_SeveralRides_SortsByCompletionAndTotalsDrivers()
    {
        TaxiOrder cancelled = TestData.NewOrder(5);
        cancelled.Cancel(new Cancellation("передумав", TestData.At(12)));
        _store.With(
            Completed(1, driverId: 1, TestData.At(15), 160m),
            Completed(2, driverId: 2, TestData.At(10), 90m),
            Completed(3, driverId: 1, TestData.At(14), 200m),
            Completed(4, driverId: 2, TestData.At(21), 300m),
            cancelled);

        ShiftReport report = Sut.Build(_dayShift);

        int[] byCompletionTime = [2, 3, 1];
        string[] byRevenue = ["Водій 1", "Водій 2"];
        Assert.Equal(byCompletionTime, report.Rows.Select(row => row.OrderId));
        Assert.Equal(byRevenue, report.Drivers.Select(total => total.DriverName));
        Assert.Equal(360m, report.Drivers[0].Revenue);
        Assert.Equal(450m, report.Revenue);
        Assert.Equal(150m, report.AverageFare);
        Assert.Equal(30m, report.TotalKm);
        Assert.Equal(1, report.CancelledCount);
    }

    private static TaxiOrder Completed(int id, int driverId, DateTimeOffset at, decimal fare)
    {
        TaxiOrder order = TestData.NewOrder(id);
        order.Assign(new DriverAssignment(driverId, 1m, at.AddHours(-1)));
        order.Start(at.AddMinutes(-30));
        order.Complete(new TripResult(10m, fare, at));
        return order;
    }
}
