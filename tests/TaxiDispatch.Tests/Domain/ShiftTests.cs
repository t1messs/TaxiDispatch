using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.Tests.Domain;

// Межі змін: специфікація, таблиця 4.4.
public class ShiftTests
{
    private static readonly Shift _day = new(new DateOnly(2026, 11, 17), ShiftKind.Day);

    [Theory]
    [InlineData(7, 59, false)]
    [InlineData(8, 0, true)]
    [InlineData(19, 59, true)]
    [InlineData(20, 0, false)]
    public void Contains_DayShiftBoundary_MatchesTable(int hour, int minute, bool expected)
    {
        var moment = new DateTimeOffset(2026, 11, 17, hour, minute, 0, TimeSpan.FromHours(2));

        Assert.Equal(expected, _day.Contains(moment));
    }

    [Fact]
    public void Contains_TimeAfterMidnight_BelongsToNightShiftOfPreviousDate()
    {
        var night = new Shift(new DateOnly(2026, 11, 17), ShiftKind.Night);

        Assert.True(night.Contains(TestData.At(3, day: 18)));
        Assert.False(night.Contains(TestData.At(8, day: 18)));
    }
}
