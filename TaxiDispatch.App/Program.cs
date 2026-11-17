using System.Text;
using Microsoft.Extensions.Logging;
using TaxiDispatch.App;
using TaxiDispatch.Core;
using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;
using TaxiDispatch.Core.Input;
using TaxiDispatch.Core.Pricing;
using TaxiDispatch.Core.Reporting;
using TaxiDispatch.Core.Services;
using TaxiDispatch.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"TaxiDispatch v{VersionInfo.Current()}");

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "HH:mm:ss ";
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
});
ILogger log = loggerFactory.CreateLogger("TaxiDispatch.App");

// Верхній обробник — єдине місце, де дозволено catch (Exception).
try
{
    RunApplication(loggerFactory);
    return 0;
}
catch (DomainRuleException ex)
{
    log.LogWarning("Правило {Rule}: {Message}", ex.Rule, ex.Message);
    Console.WriteLine($"Порушено правило: {ex.Message}");
    return 1;
}
catch (UnauthorizedAccessException ex)
{
    // Помилка середовища: немає прав на файл звіту чи аудиту.
    log.LogError(ex, "Немає доступу до файлу");
    Console.WriteLine("Немає доступу до файлу звіту або журналу аудиту.");
    return 3;
}
catch (IOException ex)
{
    // Помилка середовища: файл заблоковано, диск переповнено тощо.
    log.LogError(ex, "Збій файлової системи");
    Console.WriteLine("Не вдалося записати файл: він зайнятий або на диску немає місця.");
    return 3;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій, роботу завершено");
    Console.WriteLine("Непередбачений збій. Подробиці — у журналі.");
    return 2;
}

static void RunApplication(ILoggerFactory loggerFactory)
{
    // Композиційний корінь: єдине місце, де створюються конкретні реалізації.
    // Таблиця відповідності замість ланцюга if за типом клієнта (OCP).
    var policies = new Dictionary<string, IPricingPolicy>
    {
        ["regular"] = new DiscountPricingPolicy(0.05m),
        ["vip"] = new VipPricingPolicy(),
        ["staff"] = new DiscountPricingPolicy(0.30m),
    };

    string customerType = "regular";
    IPricingPolicy pricing = policies.TryGetValue(customerType, out IPricingPolicy? found)
        ? found
        : new StandardPricingPolicy();

    var repository = new InMemoryTaxiOrderRepository();
    var service = new TaxiOrderService(
        repository,
        pricing,
        new ConsoleNotifier(),
        loggerFactory.CreateLogger<TaxiOrderService>());

    // using гарантує Dispose (закриття файлу) навіть тоді, коли далі кинуто виняток.
    using var audit = new FileAuditLog("audit.log");

    var passenger = new Passenger(100, "Іваненко", "+380501234567");
    var order = new TaxiOrder(1, passenger.Id, DateOnly.FromDateTime(DateTime.Today));
    service.Place(order, passenger);
    audit.Write("Place", order.Id);

    ReadSegments(service, order.Id);

    if (order.Segments.Count == 0)
    {
        Console.WriteLine("Маршрут порожній — замовлення не оформлено.");
    }
    else
    {
        service.Assign(order.Id);
        audit.Write("Assign", order.Id);
        Console.WriteLine($"Сума: {service.TotalOf(order):0.00}");

        order.Start();
        order.Complete();
        audit.Write("Complete", order.Id);
        var archived = new ArchivedTaxiOrder(order, DateOnly.FromDateTime(DateTime.Today));
        Console.WriteLine($"Архів: замовлення {archived.Id}, стан {archived.Status}");

        Console.Write(TaxiOrderCsvReport.BuildCsv(repository.GetAll(), service.TotalOf));
        ReportFileExporter.Export("report.csv", repository.GetAll(), service.TotalOf);
        audit.Write("Export", order.Id);
        Console.WriteLine("Звіт збережено у report.csv, аудит — у audit.log");

        // Пасажир намагається скасувати вже завершену поїздку: правило порушено,
        // це помилка користувача — повідомляємо й працюємо далі.
        try
        {
            service.Cancel(order.Id, "пасажир передумав");
        }
        catch (DomainRuleException ex)
        {
            Console.WriteLine($"Скасування неможливе: {ex.Message}");
        }
    }
}

// Зовнішній шар: читає рядки, барикада перетворює їх на RouteSegment.
// Помилка вводу — очікувана ситуація: пояснюємо й питаємо ще раз, застосунок не падає.
static void ReadSegments(TaxiOrderService service, int orderId)
{
    Console.WriteLine("Введіть відрізки маршруту; щоб завершити, введіть «готово» замість адреси.");
    while (true)
    {
        Console.Write("Адреса призначення: ");
        string? destination = Console.ReadLine();
        if (destination is null
            || destination.Trim().Equals("готово", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Console.Write("Дистанція, км: ");
        string? distance = Console.ReadLine();
        Console.Write("Ціна за кілометр, грн: ");
        string? price = Console.ReadLine();

        Result<RouteSegment> parsed = RouteSegmentParser.Parse(destination, distance, price);
        if (!parsed.IsSuccess)
        {
            Console.WriteLine($"Помилка вводу: {parsed.Error}");
            continue; // повторюємо запит, застосунок живий
        }

        RouteSegment segment = parsed.Value!;
        service.AddSegment(orderId, segment);
        Console.WriteLine($"Відрізок додано: {segment.Destination}, {segment.Amount:0.00} грн");
    }
}
