using TaxiDispatch.Core.Documents;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Pricing;
using TaxiDispatch.Core.Reporting;
using TaxiDispatch.Core.Storage;

namespace TaxiDispatch.Tests;

// ЛР7, завдання 6: політики ціни, сховище в пам'яті, CSV-звіт і знижка за обсяг.
public class PricingAndStorageTests
{
    private static readonly RouteSegment _segment = new("Вокзал", 3m, 33.33m);

    [Fact]
    public void StandardPolicy_PriceIsAmountRounded()
    {
        decimal price = new StandardPricingPolicy().PriceOf(_segment);

        Assert.Equal(99.99m, price);
    }

    [Fact]
    public void VipPolicy_PriceIsTenPercentLess()
    {
        decimal price = new VipPricingPolicy().PriceOf(_segment);

        Assert.Equal(89.99m, price);
    }

    [Fact]
    public void InMemoryRepository_AddAndFind_ReturnsAll()
    {
        var repository = new InMemoryTaxiOrderRepository();
        TaxiOrder first = new(1, 100, new DateOnly(2026, 11, 17));
        TaxiOrder second = new(2, 100, new DateOnly(2026, 11, 17));

        repository.Add(first);
        repository.Add(second);

        Assert.Same(second, repository.GetById(2));
        Assert.Null(repository.GetById(3));
        Assert.Equal(2, repository.GetAll().Count);
    }

    [Fact]
    public void InMemoryRepository_DuplicateId_Throws()
    {
        var repository = new InMemoryTaxiOrderRepository();
        repository.Add(new TaxiOrder(1, 100, new DateOnly(2026, 11, 17)));

        Assert.Throws<InvalidOperationException>(
            () => repository.Add(new TaxiOrder(1, 200, new DateOnly(2026, 11, 17))));
    }

    [Fact]
    public void CsvReport_TwoOrders_HeaderAndOneLinePerOrder()
    {
        TaxiOrder first = new(1, 100, new DateOnly(2026, 11, 17));
        TaxiOrder second = new(2, 100, new DateOnly(2026, 11, 17));
        second.Cancel();

        string csv = TaxiOrderCsvReport.BuildCsv([first, second], order => order.Id * 50.5m);
        string[] lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        string[] expected = ["id;total;status", "1;50.50;New", "2;101.00;Cancelled"];
        Assert.Equal(expected, lines);
    }

    // Названа гілка зі звіту покриття: VolumeDiscountOf, умови amount > 5000 і amount > 2000.
    [Theory]
    [InlineData(2000, 0)]
    [InlineData(2000.01, 100.0005)]
    [InlineData(5000, 250)]
    [InlineData(5000.01, 500.001)]
    public void VolumeDiscountOf_Thresholds_MatchRules(double amount, double expected)
    {
        decimal actual = DocumentTotalCalculator.VolumeDiscountOf((decimal)amount);

        Assert.Equal((decimal)expected, actual);
    }
}
