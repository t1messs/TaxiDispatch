using TaxiDispatch.Domain.Errors;
using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Tests.Domain;

public class DriverTests
{
    [Fact]
    public void TakeOrder_BusyDriver_Throws()
    {
        Driver driver = TestData.FreeDriver(1, TestData.Center);
        driver.TakeOrder();

        var error = Assert.Throws<DomainRuleException>(driver.TakeOrder);

        Assert.Equal("driver.free", error.Rule);
    }

    [Fact]
    public void Release_BusyDriver_BecomesFreeAtNewLocation()
    {
        Driver driver = TestData.FreeDriver(1, TestData.Center);
        driver.TakeOrder();

        driver.Release(TestData.Airport.Location);

        Assert.Equal(DriverStatus.Free, driver.Status);
        Assert.Equal(TestData.Airport.Location, driver.Location);
    }
}
