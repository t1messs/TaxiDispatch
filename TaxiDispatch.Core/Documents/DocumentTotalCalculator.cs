using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Розрахунок суми документа: знижки, подача авто й наступний стан. Методи не змінюють стан.
/// </summary>
public static class DocumentTotalCalculator
{
    /// <summary>Обчислює суму позицій з урахуванням знижки за довгий відрізок.</summary>
    /// <param name="segments">Позиції документа.</param>
    /// <returns>Сума позицій, грн, без округлення.</returns>
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

    /// <summary>Обчислює знижку постійному пасажиру.</summary>
    /// <param name="amount">Сума, грн.</param>
    /// <param name="isRegularClient">Ознака постійного пасажира.</param>
    /// <returns>Знижка, грн.</returns>
    public static decimal ClientDiscountOf(decimal amount, bool isRegularClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularClient ? amount * PricingRules.RegularClientRate : 0m;
    }

    /// <summary>Обчислює знижку за великий обсяг документа.</summary>
    /// <param name="amount">Сума, грн.</param>
    /// <returns>Знижка, грн.</returns>
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

    /// <summary>Обчислює знижку за купоном.</summary>
    /// <param name="amount">Сума, грн.</param>
    /// <param name="couponCode">Код купона або <c>null</c>.</param>
    /// <returns>Знижка, грн; для невідомого купона — нуль.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom =>
                PricingRules.CouponFixed,
            _ => 0m
        };
    }

    /// <summary>Обчислює знижку для документа, створеного в неділю.</summary>
    /// <param name="amount">Сума, грн.</param>
    /// <param name="createdAt">Дата створення документа.</param>
    /// <returns>Знижка, грн.</returns>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.WeekendRate : 0m;
    }

    /// <summary>Обчислює плату за подачу авто.</summary>
    /// <param name="amount">Сума після знижок, грн.</param>
    /// <param name="deliveryPrice">Тариф подачі, грн.</param>
    /// <returns>Плата за подачу, грн; від порогу — нуль.</returns>
    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom ? deliveryPrice : 0m;
    }

    /// <summary>Визначає стан документа після розрахунку.</summary>
    /// <param name="current">Поточний стан.</param>
    /// <param name="total">Сума до сплати, грн.</param>
    /// <returns>Наступний стан документа.</returns>
    public static DocumentStatus NextStatusOf(DocumentStatus current, decimal total) =>
        current switch
        {
            DocumentStatus.New when total > 0m => DocumentStatus.Paid,
            DocumentStatus.Paid => DocumentStatus.Shipped,
            _ => current
        };

    /// <summary>Розраховує документ повністю: суму до сплати та наступний стан.</summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <returns>Сума, округлена до копійок, і стан документа.</returns>
    public static TotalResult Calculate(TotalRequest request)
    {
        DocumentGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Segments);
        total -= ClientDiscountOf(total, request.Client.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= WeekendDiscountOf(total, request.CreatedAt);
        total += DeliveryFeeOf(total, request.DeliveryPrice);

        total = request.Status == DocumentStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2, MidpointRounding.AwayFromZero);
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }
}
