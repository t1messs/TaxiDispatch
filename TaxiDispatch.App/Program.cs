using System.Text;
using TaxiDispatch.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var order = new TaxiOrder(1, 100, DateOnly.FromDateTime(DateTime.Today));
order.AddSegment(new RouteSegment("Вулиця 1 - Вулиця 2", 3m, 250m));
order.AddSegment(new RouteSegment("Вулиця 2 - Вулиця 3", 12m, 90m));
order.Assign();

Console.WriteLine($"Замовлення {order.Id}: відрізків {order.Segments.Count}, стан {order.Status}");
