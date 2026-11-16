namespace TaxiDispatch.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, PassengerKind kind, int tripCount)
    {
        decimal rate = RateFor(kind, tripCount);
        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }

    private static decimal RateFor(PassengerKind kind, int tripCount) => kind switch
    {
        PassengerKind.Vip => PricingRules.VipRate,
        PassengerKind.Staff => PricingRules.StaffRate,
        _ => tripCount > PricingRules.LoyalTrips ? PricingRules.LoyalRate : 0m,
    };
}
