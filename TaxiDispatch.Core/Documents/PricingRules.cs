namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Параметри правил ціни модуля документів.
/// </summary>
public static class PricingRules
{
    /// <summary>Відстань відрізка, км, від якої діє знижка за довгий відрізок.</summary>
    public const int BulkDistance = 10;

    /// <summary>Знижка за довгий відрізок.</summary>
    public const decimal BulkRate = 0.05m;

    /// <summary>Знижка постійному пасажиру.</summary>
    public const decimal RegularClientRate = 0.07m;

    /// <summary>Сума, грн, вище якої документ вважається великим.</summary>
    public const decimal LargeOrderFrom = 5000m;

    /// <summary>Знижка за великий документ.</summary>
    public const decimal LargeOrderRate = 0.10m;

    /// <summary>Сума, грн, вище якої документ вважається середнім.</summary>
    public const decimal MediumOrderFrom = 2000m;

    /// <summary>Знижка за середній документ.</summary>
    public const decimal MediumOrderRate = 0.05m;

    /// <summary>Знижка для документа, створеного в неділю.</summary>
    public const decimal WeekendRate = 0.02m;

    /// <summary>Знижка за купоном SALE10.</summary>
    public const decimal CouponSaleRate = 0.10m;

    /// <summary>Фіксована знижка за купоном MINUS200, грн.</summary>
    public const decimal CouponFixed = 200m;

    /// <summary>Сума, грн, вище якої діє купон MINUS200.</summary>
    public const decimal CouponFixedFrom = 1000m;

    /// <summary>Сума, грн, від якої подача авто безкоштовна.</summary>
    public const decimal FreeDeliveryFrom = 1000m;

    /// <summary>Код купона з відсотковою знижкою.</summary>
    public const string SaleCoupon = "SALE10";

    /// <summary>Код купона з фіксованою знижкою.</summary>
    public const string FixedCoupon = "MINUS200";
}
