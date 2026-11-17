using TaxiDispatch.Core.Pricing;

namespace TaxiDispatch.Tests;

public class RideDiscountPolicyTests
{
    [Fact]
    public void DiscountFor_NegativeAmount_Throws()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        void Act() => policy.DiscountFor(-0.01m, true);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void DiscountFor_ZeroAmountNewPassenger_ReturnsZero()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(0m, false);

        // Assert
        Assert.Equal(0.00m, actual);
    }

    [Fact]
    public void DiscountFor_ShortRideRegular_ReturnsFivePercent()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(400m, true);

        // Assert
        Assert.Equal(20.00m, actual);
    }

    [Fact]
    public void DiscountFor_JustBelowLongRideNewPassenger_ReturnsZero()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(999.99m, false);

        // Assert
        Assert.Equal(0.00m, actual);
    }

    [Fact]
    public void DiscountFor_JustBelowLongRideRegular_ReturnsFivePercent()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(999.99m, true);

        // Assert
        Assert.Equal(50.00m, actual);
    }

    [Fact]
    public void DiscountFor_LongRideBoundaryNewPassenger_ReturnsTenPercent()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(1000m, false);

        // Assert
        Assert.Equal(100.00m, actual);
    }

    [Fact]
    public void DiscountFor_LongRideBoundaryRegular_ReturnsFifteenPercent()
    {
        // Arrange
        var policy = new RideDiscountPolicy();

        // Act
        decimal actual = policy.DiscountFor(1000m, true);

        // Assert
        Assert.Equal(150.00m, actual);
    }
}
