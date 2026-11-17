using TaxiDispatch.Core.Pricing;

namespace TaxiDispatch.Tests;

public class RideDiscountPolicyTests
{
    // decimal не є константою часу компіляції, тому суми передаємо як double
    // і перетворюємо на decimal у тілі тесту.
    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(400, true, 20)]
    [InlineData(999.99, false, 0)]
    [InlineData(999.99, true, 50)]
    [InlineData(1000, false, 100)]
    [InlineData(1000, true, 150)]
    public void DiscountFor_Boundaries_MatchesTable(double amount, bool isRegular, double expected)
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor((decimal)amount, isRegular);

        // Assert
        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void DiscountFor_NegativeAmount_ThrowsOutOfRange(double amount)
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => policy.DiscountFor((decimal)amount, true));

        // Assert
        Assert.Equal("amount", ex.ParamName);
    }
}
