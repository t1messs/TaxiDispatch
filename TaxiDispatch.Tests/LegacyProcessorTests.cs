using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;
using Xunit;
using TaxiDispatch.Core.Documents;


namespace TaxiDispatch.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularClient_Returns1292()
    {
        List<RouteSegment> segments = new()
        {
            new RouteSegment { Destination = "A1", Distance = 12, PricePerKm = 100m },
            new RouteSegment { Destination = "B2", Distance = 1, PricePerKm = 250m }
        };

        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, segments,
            new DateOnly(2026, 3, 10), "", "New", false,
            60m, out string next);

        Assert.Equal(1292.70m, total);
        Assert.Equal("Paid", next);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<RouteSegment> segments = new()
        {
            new RouteSegment { Destination = "A1", Distance = 2, PricePerKm = 200m }
        };

        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, segments,
            new DateOnly(2026, 3, 10), "SALE10", "Paid",
            false, 60m, out string next);

        Assert.Equal(420m, total);
        Assert.Equal("Shipped", next);
    }

    [Fact]
    public void Calculate_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() =>
            DocumentGuards.EnsureLinesValid(new List<RouteSegment>()));
    }

    [Fact]
    public void Calculate_ZeroDistance_ThrowsRange()
    {
        RouteSegment segment = new()
        {
            Destination = "A1",
            Distance = 0,
            PricePerKm = 10m
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => DocumentGuards.EnsureLineValid(segment));
    }

}
