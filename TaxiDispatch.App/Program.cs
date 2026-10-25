using System.Globalization;
using TaxiDispatch.Core.Domain;

TaxiOrder order = new()
{
    Id = 101,
    PassengerId = 5,
    CreatedAt = DateTimeOffset.Now,
};
order.AddSegment(new RouteSegment
{
    DestinationAddress = "Залізничний вокзал",
    DistanceKm = 5.5m,
    PricePerKm = 15.0m,
});
order.AddSegment(new RouteSegment
{
    DestinationAddress = "Аеропорт",
    DistanceKm = 12.0m,
    PricePerKm = 14.5m,
});
string total = order.Total().ToString("F2", CultureInfo.InvariantCulture);
Console.WriteLine($"Замовлення таксі #{order.Id}");
Console.WriteLine($"Стан: {order.Status}");
Console.WriteLine($"Відрізків маршруту: {order.Segments.Count}");
Console.WriteLine($"Загальна вартість: {total} грн");
