using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;
using TaxiDispatch.Core.Input;

namespace TaxiDispatch.Tests;

// ЛР8, завдання 6: правила валідації зовнішнього вводу на межі модуля.
public class RouteSegmentParserTests
{
    public static TheoryData<string?, string?, string?> InvalidInputs => new()
    {
        { null, "3", "20" },
        { "   ", "3", "20" },
        { new string('А', 101), "3", "20" },
        { "Вокзал", "abc", "20" },
        { "Вокзал", "0", "20" },
        { "Вокзал", "-2", "20" },
        { "Вокзал", "1000,01", "20" },
        { "Вокзал", "3", "-1" },
        { "Вокзал", "3", "500,01" },
        { "Вокзал", "3", null },
    };

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void Parse_InvalidInput_ReturnsFailure(
        string? destination,
        string? distance,
        string? price)
    {
        // Act
        Result<RouteSegment> result = RouteSegmentParser.Parse(destination, distance, price);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Error);
    }

    [Theory]
    [InlineData("  Вокзал ", "3,5", "20", 3.5, 20)]
    [InlineData("Вокзал", "3.5", "20", 3.5, 20)]
    [InlineData("Аеропорт", "1000", "500", 1000, 500)]
    [InlineData("Центр", "0,1", "0", 0.1, 0)]
    public void Parse_ValidInput_ReturnsSegment(
        string destination,
        string distance,
        string price,
        double expectedKm,
        double expectedPrice)
    {
        Result<RouteSegment> result = RouteSegmentParser.Parse(destination, distance, price);

        Assert.True(result.IsSuccess);
        Assert.Equal(destination.Trim(), result.Value!.Destination);
        Assert.Equal((decimal)expectedKm, result.Value.DistanceKm);
        Assert.Equal((decimal)expectedPrice, result.Value.PricePerKm);
    }
}
