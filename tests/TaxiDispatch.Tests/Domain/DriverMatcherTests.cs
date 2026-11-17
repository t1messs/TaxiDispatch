using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Tests.Domain;

// Профільне правило, частина 2: специфікація, таблиця 4.2. Подача — TestData.Center.
public class DriverMatcherTests
{
    [Fact]
    public void Choose_TwoCandidates_ReturnsNearest()
    {
        Driver near = TestData.FreeDriver(1, TestData.North(0.01));
        Driver far = TestData.FreeDriver(2, TestData.North(0.02));

        DriverMatch? match = DriverMatcher.Choose([far, near], TestData.Center, CarClass.Economy);

        Assert.Same(near, match?.Driver);
        Assert.Equal(1.112m, match?.PickupKm);
    }

    [Fact]
    public void Choose_EqualDistance_PrefersHigherRating()
    {
        Driver north = TestData.FreeDriver(1, TestData.North(0.01), rating: 4.2m);
        Driver south = TestData.FreeDriver(2, TestData.North(-0.01), rating: 4.9m);

        DriverMatch? match = DriverMatcher.Choose(
            [north, south],
            TestData.Center,
            CarClass.Economy);

        Assert.Same(south, match?.Driver);
    }

    [Fact]
    public void Choose_DriverExactlyAtRadius_IsCandidate()
    {
        // 0,044966° широти = 4,999998 км → 5,000 км після округлення до метра.
        Driver atRadius = TestData.FreeDriver(1, TestData.North(0.044966));

        DriverMatch? match = DriverMatcher.Choose([atRadius], TestData.Center, CarClass.Economy);

        Assert.Equal(DriverMatcher.MaxPickupKm, match?.PickupKm);
    }

    [Fact]
    public void Choose_DriverJustBeyondRadius_ReturnsNull()
    {
        // 0,04498° широти = 5,002 км: на 2 м далі за межу.
        Driver beyond = TestData.FreeDriver(1, TestData.North(0.04498));

        DriverMatch? match = DriverMatcher.Choose([beyond], TestData.Center, CarClass.Economy);

        Assert.Null(match);
    }

    [Fact]
    public void Choose_BusyOrOtherClass_ReturnsNull()
    {
        Driver busy = TestData.FreeDriver(1, TestData.North(0.01));
        busy.TakeOrder();
        Driver comfort = TestData.FreeDriver(2, TestData.North(0.01), CarClass.Comfort);

        DriverMatch? match = DriverMatcher.Choose(
            [busy, comfort],
            TestData.Center,
            CarClass.Economy);

        Assert.Null(match);
    }
}
