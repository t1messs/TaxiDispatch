using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Pricing;
using TaxiDispatch.Core.Services;
using TaxiDispatch.Core.Storage;
using Xunit;

namespace TaxiDispatch.Tests;

public class DomainInvariantTests
{
    [Fact]
    public void RouteSegment_WithZeroDistance_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RouteSegment("Вулиця 1 - Вулиця 2", 0m, 100m));
    }

    [Fact]
    public void TaxiOrder_CompleteWhenNew_ThrowsInvalidOperation()
    {
        TaxiOrder order = NewOrder(1);
        order.AddSegment(new RouteSegment("Вулиця 1 - Вулиця 2", 2m, 100m));

        Assert.Throws<InvalidOperationException>(() => order.Complete());
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns180()
    {
        TaxiOrderService service = new(
            new InMemoryTaxiOrderRepository(),
            new DiscountPricingPolicy(0.10m),
            new NullNotifier());
        TaxiOrder order = NewOrder(2);
        order.AddSegment(new RouteSegment("Вулиця 1 - Вулиця 2", 2m, 100m));

        Assert.Equal(180m, service.TotalOf(order));
    }

    private static TaxiOrder NewOrder(int id) =>
        new(id, 100, new DateOnly(2026, 11, 12));

    // Сповіщувач-заглушка: тест не залежить від пошти чи консолі.
    private sealed class NullNotifier : INotifier
    {
        public void Notify(string recipient, string message)
        {
        }
    }
}
