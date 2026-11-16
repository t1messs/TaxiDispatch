using System.Text;
using TaxiDispatch.App;
using TaxiDispatch.App.Input;
using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;
using TaxiDispatch.Core.Pricing;
using TaxiDispatch.Core.Reporting;
using TaxiDispatch.Core.Services;
using TaxiDispatch.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

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
var service = new TaxiOrderService(repository, pricing, new ConsoleNotifier());

var passenger = new Passenger(100, "Іваненко", "+380501234567");
var order = new TaxiOrder(1, passenger.Id, DateOnly.FromDateTime(DateTime.Today));
service.Place(order, passenger);

ReadSegments(service, order.Id);

if (order.Segments.Count == 0)
{
    Console.WriteLine("Маршрут порожній — замовлення не оформлено.");
}
else
{
    service.Assign(order.Id);
    Console.WriteLine($"Сума: {service.TotalOf(order):0.00}");

    order.Start();
    order.Complete();
    var archived = new ArchivedTaxiOrder(order, DateOnly.FromDateTime(DateTime.Today));
    Console.WriteLine($"Архів: замовлення {archived.Id}, стан {archived.Status}");

    Console.Write(TaxiOrderCsvReport.BuildCsv(repository.GetAll(), service.TotalOf));
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
