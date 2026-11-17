using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Application.Queries;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.App;

/// <summary>
/// Екрани UC-05 «Знайти замовлення», UC-06 «Звіт за зміну» й довідник місць.
/// </summary>
internal sealed class ReportScreens
{
    private readonly ConsoleIo _io;
    private readonly IPlaceDirectory _places;
    private readonly OrderQueryService _query;
    private readonly ShiftReportService _reports;

    public ReportScreens(
        ConsoleIo io,
        IPlaceDirectory places,
        OrderQueryService query,
        ShiftReportService reports)
    {
        _io = io;
        _places = places;
        _query = query;
        _reports = reports;
    }

    public void Find()
    {
        _io.Say("  Порожній ввід — критерій не застосовується.");
        var criteria = new OrderSearchCriteria
        {
            Status = _io.Ask(
                "Стан (New, Assigned, InProgress, Completed, Cancelled)",
                InputParser.ParseOptionalStatus),
            CreatedOn = _io.Ask("Дата створення (дд.мм.рррр)", InputParser.ParseOptionalDate),
            Phone = _io.Ask("Телефон пасажира", InputParser.ParseOptionalPhone),
        };
        Result<IReadOnlyList<TaxiOrder>> result = _query.Find(criteria);
        if (!result.IsSuccess)
        {
            _io.Say($"  ! {result.Error}");
            return;
        }

        IReadOnlyList<TaxiOrder> orders = result.Value!;
        _io.Say(orders.Count == 0 ? "  Замовлень не знайдено." : $"  Знайдено: {orders.Count}");
        foreach (TaxiOrder order in orders)
        {
            TripPlan plan = order.Plan;
            _io.Say(
                $"  № {order.Id,-4} {Formats.Time(order.CreatedAt)}  {plan.Pickup.Code} → "
                + $"{plan.Dropoff.Code}  {plan.CarClass,-8} {order.Status,-10} "
                + (order.Result is null ? string.Empty : Formats.Money(order.Result.Fare)));
        }
    }

    public void ShiftReport()
    {
        DateOnly date = _io.Ask("Дата початку зміни (дд.мм.рррр)", InputParser.ParseDate);
        ShiftKind kind = _io.Ask("Зміна («д» — денна, «н» — нічна)", InputParser.ParseShiftKind);
        ShiftReportPrinter.Print(_io, _reports.Build(new Shift(date, kind)));
    }

    public void Places()
    {
        foreach (var place in _places.All())
        {
            _io.Say($"  {place.Code}  {place.Name}");
        }
    }
}
