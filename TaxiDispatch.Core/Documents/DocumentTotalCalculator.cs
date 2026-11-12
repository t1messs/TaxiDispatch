using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

public static class DocumentTotalCalculator
{
    public static decimal SubtotalOf(IReadOnlyList<RouteSegment> segments)
    {
        DocumentGuards.EnsureLinesValid(segments);
        decimal subtotal = 0m;
        foreach (RouteSegment segment in segments)
        {
            decimal sum = segment.Distance * segment.PricePerKm;
            if (segment.Distance >= PricingRules.BulkDistance)
            {
                sum -= sum * PricingRules.BulkRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    public static decimal ClientDiscountOf(decimal amount, bool isRegularClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularClient ? amount * PricingRules.RegularClientRate : 0m;
    }

    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
    }

    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom => PricingRules.CouponFixed,
            _ => 0m
        };
    }

    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.WeekendRate : 0m;
    }

    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom ? deliveryPrice : 0m;
    }

    public static DocumentStatus NextStatusOf(DocumentStatus current, decimal total) =>
        current switch
        {
            DocumentStatus.New when total > 0m => DocumentStatus.Paid,
            DocumentStatus.Paid => DocumentStatus.Shipped,
            _ => current
        };

    public static TotalResult Calculate(TotalRequest request)
    {
        DocumentGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Segments);
        total -= ClientDiscountOf(total, request.Client.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= WeekendDiscountOf(total, request.CreatedAt);
        total += DeliveryFeeOf(total, request.DeliveryPrice);

        total = request.Status == DocumentStatus.Cancelled ? 0m : Math.Round(Math.Max(total, 0m), 2);
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }
}
