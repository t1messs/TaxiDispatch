using System.Text;
using TaxiDispatch.App;
using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;
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
order.AddSegment(new RouteSegment("Вулиця 1 - Вулиця 2", 2m, 250m));
service.Place(order, passenger);

Console.WriteLine($"Сума: {service.TotalOf(order):0.00}");

order.Assign();
order.Start();
order.Complete();
var archived = new ArchivedTaxiOrder(order, DateOnly.FromDateTime(DateTime.Today));
Console.WriteLine($"Архів: замовлення {archived.Id}, стан {archived.Status}");

Console.Write(TaxiOrderCsvReport.BuildCsv(repository.GetAll(), service.TotalOf));
