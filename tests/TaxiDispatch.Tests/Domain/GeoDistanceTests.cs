using TaxiDispatch.Domain.Geo;

namespace TaxiDispatch.Tests.Domain;

// Стандарт кодування 2.0.0, правила X-02…X-05: координати, відстані, точність, одиниці.
public class GeoDistanceTests
{
    private static readonly GeoPoint _kyiv = new(50.4501, 30.5234);
    private static readonly GeoPoint _lviv = new(49.8397, 24.0297);

    [Fact]
    public void KilometersBetween_KyivLviv_Is467Point531()
    {
        decimal distanceKm = GeoDistance.KilometersBetween(_kyiv, _lviv);

        Assert.Equal(467.531m, distanceKm);
    }

    [Fact]
    public void KilometersBetween_OneDegreeOnEquator_Is111Point195()
    {
        decimal distanceKm = GeoDistance.KilometersBetween(new GeoPoint(0, 0), new GeoPoint(0, 1));

        Assert.Equal(111.195m, distanceKm);
    }

    [Fact]
    public void KilometersBetween_SwappedPoints_IsSymmetric()
    {
        decimal there = GeoDistance.KilometersBetween(_kyiv, _lviv);
        decimal back = GeoDistance.KilometersBetween(_lviv, _kyiv);

        Assert.Equal(there, back);
    }

    [Fact]
    public void KilometersBetween_SamePoint_IsZero()
    {
        Assert.Equal(0m, GeoDistance.KilometersBetween(_kyiv, _kyiv));
    }

    [Theory]
    [InlineData(90.0001, 0)]
    [InlineData(-90.0001, 0)]
    [InlineData(0, 180.0001)]
    [InlineData(0, -180.0001)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void GeoPoint_OutOfRange_Throws(double latitude, double longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoPoint(latitude, longitude));
    }

    [Theory]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    public void GeoPoint_Boundaries_AreAllowed(double latitude, double longitude)
    {
        var point = new GeoPoint(latitude, longitude);

        Assert.Equal(latitude, point.Latitude);
        Assert.Equal(longitude, point.Longitude);
    }

    [Fact]
    public void IsNear_PointsWithinTolerance()
    {
        // Точки на одному меридіані, різниця широти 0,0004° ≈ 44 м.
        var entrance = new GeoPoint(50.4501, 30.5234);
        var parking = new GeoPoint(50.4505, 30.5234);

        Assert.True(GeoDistance.IsNear(entrance, parking, 0.05m));
        Assert.False(GeoDistance.IsNear(entrance, parking, 0.04m));
    }
}
