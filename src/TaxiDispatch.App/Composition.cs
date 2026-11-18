using Microsoft.Extensions.Logging;
using TaxiDispatch.Application.Dispatch;
using TaxiDispatch.Application.Orders;
using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Application.Queries;
using TaxiDispatch.Infrastructure.Json;

namespace TaxiDispatch.App;

/// <summary>
/// Композиційний корінь: єдине місце, де створюються конкретні реалізації й з'єднуються
/// шари. Решта коду знає лише інтерфейси, тому сховище чи годинник підмінюються тут.
/// </summary>
internal static class Composition
{
    public static MainMenu CreateMenu(
        ConsoleIo io,
        ILoggerFactory logs,
        TimeProvider clock,
        FeatureFlags flags)
    {
        var paths = new DataPaths("data", "state");
        var store = new JsonTaxiStore(paths);
        IFarePolicy fares = new TariffFarePolicy(JsonTariffSource.Load(paths));
        if (flags.SurgeFactor is decimal surge)
        {
            fares = new SurgeFarePolicy(fares, surge);
        }

        var orders = new OrderScreens(
            io,
            store.Places,
            new OrderIntakeService(store, fares, clock, logs.CreateLogger<OrderIntakeService>()),
            new DispatchService(store, fares, clock, logs.CreateLogger<DispatchService>()));
        var reports = new ReportScreens(
            io,
            store.Places,
            new OrderQueryService(store, logs.CreateLogger<OrderQueryService>()),
            new ShiftReportService(store, logs.CreateLogger<ShiftReportService>()));
        return new MainMenu(io, orders, reports);
    }
}
