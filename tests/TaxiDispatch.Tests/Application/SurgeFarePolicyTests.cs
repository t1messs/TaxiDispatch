using TaxiDispatch.Application.Pricing;
using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Tests.Application;

// Функція «коефіцієнт попиту» за прапорцем: декоратор поверх тарифної політики.
// День, економ, 10 км: за тарифом 40 + 12 × 10 = 160 грн.
public class SurgeFarePolicyTests
{
    private readonly TariffFarePolicy _tariff = new(TestData.Tariffs());

    [Theory]
    [InlineData(1.0, 160.00)]
    [InlineData(1.5, 240.00)]
    [InlineData(2.0, 320.00)]
    public void Quote_DaytimeEconomy10Km_MultipliesByFactor(double factor, double expected)
    {
        var surge = new SurgeFarePolicy(_tariff, (decimal)factor);

        decimal fare = surge.Quote(CarClass.Economy, 10m, TestData.At(12));

        Assert.Equal((decimal)expected, fare);
    }

    [Theory]
    [InlineData(0.9)]
    [InlineData(2.1)]
    public void Constructor_FactorOutOfRange_Throws(double factor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SurgeFarePolicy(_tariff, (decimal)factor));
    }
}
