using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Places;
using TaxiDispatch.Domain.Shifts;
using TaxiDispatch.Tests.Doubles;

namespace TaxiDispatch.Tests.Application;

// Барикада вводу: некоректний текст дає Result з поясненням, а не виняток.
public class InputParserTests
{
    private readonly FakeStore _places = new FakeStore().With(TestData.Station, TestData.Airport);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParsePhone_EmptyOrNull_ReturnsError(string? text)
    {
        Assert.False(InputParser.ParsePhone(text).IsSuccess);
    }

    [Theory]
    [InlineData("0501234567")]
    [InlineData("+38050123456")]
    [InlineData("+38050123456a")]
    public void ParsePhone_WrongFormat_ReturnsError(string text)
    {
        Assert.False(InputParser.ParsePhone(text).IsSuccess);
    }

    [Fact]
    public void ParsePhone_WithSpaces_ReturnsCompactNumber()
    {
        Result<string> phone = InputParser.ParsePhone(" +380 50 123 45 67 ");

        Assert.Equal("+380501234567", phone.Value);
    }

    [Fact]
    public void ParsePlace_UnknownCode_ReturnsError()
    {
        Assert.False(InputParser.ParsePlace("P99", _places.Places).IsSuccess);
    }

    [Fact]
    public void ParsePlace_LowerCaseCode_FindsPlace()
    {
        Assert.Same(TestData.Station, InputParser.ParsePlace("p01", _places.Places).Value);
    }

    [Fact]
    public void ParsePlace_Coordinates_CreatesGpsPlace()
    {
        Result<Place> place = InputParser.ParsePlace("50,45; 30.52", _places.Places);

        Assert.Equal(InputParser.GpsCode, place.Value!.Code);
        Assert.Equal(50.45, place.Value.Location.Latitude);
        Assert.Equal(30.52, place.Value.Location.Longitude);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("500,1")]
    [InlineData("дванадцять")]
    public void ParseActualKm_InvalidValue_ReturnsError(string text)
    {
        Assert.False(InputParser.ParseActualKm(text).IsSuccess);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("12,5", 12.5)]
    [InlineData("500", 500.0)]
    public void ParseActualKm_EmptyOrValid_ReturnsKm(string text, double? expected)
    {
        Result<decimal?> km = InputParser.ParseActualKm(text);

        Assert.True(km.IsSuccess);
        Assert.Equal((decimal?)expected, km.Value);
    }

    [Theory]
    [InlineData("1", CarClass.Economy)]
    [InlineData("comfort", CarClass.Comfort)]
    [InlineData(" Business ", CarClass.Business)]
    public void ParseCarClass_DigitOrName_ReturnsClass(string text, CarClass expected)
    {
        Assert.Equal(expected, InputParser.ParseCarClass(text).Value);
    }

    [Theory]
    [InlineData("4")]
    [InlineData("люкс")]
    public void ParseCarClass_Unknown_ReturnsError(string text)
    {
        Assert.False(InputParser.ParseCarClass(text).IsSuccess);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("12a")]
    public void ParseOrderId_NotPositiveNumber_ReturnsError(string text)
    {
        Assert.False(InputParser.ParseOrderId(text).IsSuccess);
    }

    [Fact]
    public void ParseDateAndShift_ValidText_ReturnsValues()
    {
        Assert.Equal(new DateOnly(2026, 11, 17), InputParser.ParseDate("17.11.2026").Value);
        Assert.Equal(ShiftKind.Night, InputParser.ParseShiftKind("н").Value);
        Assert.False(InputParser.ParseDate("2026-11-17").IsSuccess);
    }
}
