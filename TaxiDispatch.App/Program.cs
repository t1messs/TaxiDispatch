using System.Text;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Pricing;
using TaxiDispatch.Core.Services;
using TaxiDispatch.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// Композиційний корінь: єдине місце, де створюються конкретні реалізації
var repository = new InMemoryTaxiOrderRepository();
var pricing = new DiscountPricingPolicy(0.05m);
var service = new TaxiOrderService(repository, pricing);

var order = new TaxiOrder(1, 100, DateOnly.FromDateTime(DateTime.Today));
order.AddSegment(new RouteSegment("Вулиця 1 - Вулиця 2", 2m, 250m));
service.Place(order);

Console.WriteLine($"Сума: {service.TotalOf(order):0.00}");
