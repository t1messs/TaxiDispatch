using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;
using Xunit;
using TaxiDispatch.Core.Documents;
using System.IO;
using TaxiDispatch.Core.Reporting;

namespace TaxiDispatch.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Calculate_RegularClient_Returns1292()
    {
        TotalRequest request = new(
            DocumentId: 1,
            Client: new ClientInfo("Іваненко", "i@ex.com", true),
            Segments: new List<RouteSegment>
            {
                new() { Destination = "A1", Distance = 12, PricePerKm = 100m },
                new() { Destination = "B2", Distance = 1, PricePerKm = 250m }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: DocumentStatus.New,
            DeliveryPrice: 60m);

        TotalResult result = DocumentTotalCalculator.Calculate(request);
        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(DocumentStatus.Paid, result.Status);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        TotalRequest request = new(
            DocumentId: 2,
            Client: new ClientInfo("Петренко", "p@ex.com", false),
            Segments: new List<RouteSegment>
            {
                new() { Destination = "A1", Distance = 2, PricePerKm = 200m }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: "SALE10",
            Status: DocumentStatus.Paid,
            DeliveryPrice: 60m);

        TotalResult result = DocumentTotalCalculator.Calculate(request);
        Assert.Equal(420m, result.Total);
        Assert.Equal(DocumentStatus.Shipped, result.Status);
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

    private static TotalRequest BuildSampleRequest()
    {
        return new TotalRequest(
            DocumentId: 1,
            Client: new ClientInfo("Іваненко", "i@ex.com", true),
            Segments: new List<RouteSegment>
            {
                new() { Destination = "A1", Distance = 12, PricePerKm = 100m },
                new() { Destination = "B2", Distance = 1, PricePerKm = 250m }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: DocumentStatus.New,
            DeliveryPrice: 60m);
    }

    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = DocumentTotalCalculator.Calculate(request);

        string text = DocumentReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        StringWriter writer = new();
        DocumentReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString());
    }
}
