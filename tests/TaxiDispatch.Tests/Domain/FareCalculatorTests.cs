using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Pricing;

namespace TaxiDispatch.Tests.Domain;

// Профільне правило, частина 1: кожен рядок таблиці рішень 4.1 — випадок теорії.
public class FareCalculatorTests
{
    private static readonly TariffBook _tariffs = TestData.Tariffs();

    [Theory]
    [InlineData(CarClass.Economy, 3.0, false, 90.00)]
    [InlineData(CarClass.Economy, 10.0, false, 160.00)]
    [InlineData(CarClass.Economy, 10.0, true, 200.00)]
    [InlineData(CarClass.Economy, 3.0, true, 112.50)]
    [InlineData(CarClass.Economy, 0.0, false, 90.00)]
    [InlineData(CarClass.Comfort, 4.0, false, 120.00)]
    [InlineData(CarClass.Comfort, 10.123, false, 216.97)]
    [InlineData(CarClass.Business, 10.0, false, 350.00)]
    [InlineData(CarClass.Business, 5.0, true, 275.00)]
    public void Calculate_DecisionTableRow_ReturnsExpectedFare(
        CarClass carClass,
        double km,
        bool isNight,
        double expected)
    {
        decimal fare = FareCalculator.Calculate(_tariffs.For(carClass), (decimal)km, isNight);

        Assert.Equal((decimal)expected, fare);
    }

    [Fact]
    public void Calculate_NegativeDistance_Throws()
    {
        Tariff economy = _tariffs.For(CarClass.Economy);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => FareCalculator.Calculate(economy, -0.001m, false));
    }

    [Theory]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    [InlineData(5, 59, true)]
    [InlineData(6, 0, false)]
    public void IsNight_BoundaryTime_MatchesTable(int hour, int minute, bool expected)
    {
        bool isNight = FareCalculator.IsNight(new TimeOnly(hour, minute));

        Assert.Equal(expected, isNight);
    }

    [Fact]
    public void TariffBook_ClassWithoutTariff_Throws()
    {
        Tariff[] onlyEconomy = [new Tariff(CarClass.Economy, 40m, 12m, 90m)];

        Assert.Throws<ArgumentException>(() => new TariffBook(onlyEconomy));
    }
}
