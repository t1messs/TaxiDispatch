using Microsoft.Extensions.Logging;
using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Сценарій UC-06: звіт «Поїздки за зміну».
/// </summary>
public sealed class ShiftReportService
{
    private const string UnknownDriver = "невідомий водій";

    private readonly ITaxiStore _store;
    private readonly ILogger<ShiftReportService> _log;

    /// <summary>
    /// Створює сервіс звіту.
    /// </summary>
    /// <param name="store">Дані диспетчерської.</param>
    /// <param name="log">Журнал сценарію.</param>
    public ShiftReportService(ITaxiStore store, ILogger<ShiftReportService> log)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>
    /// Будує звіт: поїздки, завершені в межах зміни, підсумки водіїв і кількість скасувань.
    /// Дані не змінює.
    /// </summary>
    /// <param name="shift">Зміна.</param>
    /// <returns>Звіт; для зміни без поїздок — з порожніми рядками й нульовими підсумками.</returns>
    public ShiftReport Build(Shift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);
        using IDisposable? scope = _log.BeginScope("UC-06");
        _log.LogInformation("Початок: зміна {Date}, {Kind}", shift.Date, shift.Kind);
        Dictionary<int, string> names = _store.Drivers.All()
            .ToDictionary(driver => driver.Id, driver => driver.Profile.Name);
        List<ShiftReportRow> rows = _store.Orders.All()
            .Where(order => order.Result is not null && shift.Contains(order.Result.CompletedAt))
            .Select(order => ToRow(order, names))
            .OrderBy(row => row.CompletedAt)
            .ThenBy(row => row.OrderId)
            .ToList();
        List<DriverTotal> drivers = rows
            .GroupBy(row => row.DriverId)
            .Select(group => new DriverTotal(
                group.First().DriverName,
                group.Count(),
                group.Sum(row => row.Fare)))
            .OrderByDescending(total => total.Revenue)
            .ThenBy(total => total.DriverName, StringComparer.Ordinal)
            .ToList();
        int cancelled = _store.Orders.All().Count(order =>
            order.Cancellation is not null && shift.Contains(order.Cancellation.CancelledAt));
        _log.LogInformation(
            "Завершено: поїздок {Rides}, скасувань {Cancelled}",
            rows.Count,
            cancelled);
        return new ShiftReport(shift, rows, drivers, cancelled);
    }

    private static ShiftReportRow ToRow(TaxiOrder order, Dictionary<int, string> names)
    {
        TripResult result = order.Result!;
        int driverId = order.Assignment?.DriverId ?? 0;
        return new ShiftReportRow
        {
            OrderId = order.Id,
            CompletedAt = result.CompletedAt,
            DriverId = driverId,
            DriverName = names.GetValueOrDefault(driverId, UnknownDriver),
            CarClass = order.Plan.CarClass,
            Km = result.ActualKm,
            Fare = result.Fare,
        };
    }
}
