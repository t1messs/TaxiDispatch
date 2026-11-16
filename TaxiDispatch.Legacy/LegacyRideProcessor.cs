using System;
using System.Collections.Generic;

namespace TaxiDispatch.Legacy;

public class Passenger
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public PassengerKind Kind = PassengerKind.Regular;
    public int TripCount;
    public DateTime SinceUtc;
}

public class RideLine
{
    public string Code = "";
    public int Km;
    public decimal PricePerKm;
    public string Zone = "";
}

public class LegacyRideProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();

    public string Handle(int rideId, int passengerId,
        string passengerName, string? passengerEmail,
        PassengerKind passengerKind, int tripCount,
        List<RideLine>? lines, RideState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        // Охоронні умови: порядок перевірок той самий, що й у вкладених if.
        if (lines == null) return "ERR: null";
        if (lines.Count == 0) return "ERR: empty";
        if (!IsProcessable(state)) return "ERR: state";
        if (!HasValidEmail(passengerEmail)) return "ERR: mail";

        _log.Add("ok " + rideId);

        _tmpSum = Subtotal(lines);
        _tmpDiscount = _discounts.For(_tmpSum, passengerKind, tripCount);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = _report.Build(rideId, passengerName, lines, currency,
            _tmpDiscount, ship, total);

        if (sendMail)
        {
            _log.Add("mail -> " + passengerEmail);
        }
        return txt;
    }

    private static decimal Subtotal(List<RideLine> lines)
    {
        decimal sum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            sum += lines[i].Km * lines[i].PricePerKm;
        }

        return sum;
    }

    private static bool IsProcessable(RideState state) =>
        state == RideState.New || state == RideState.Paid;

    private static bool HasValidEmail(string? email) =>
        email != null && email.Contains("@");

    public decimal Preview(PassengerKind passengerKind, int tripCount,
        List<RideLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal payable = subtotal - _discounts.For(subtotal, passengerKind, tripCount);
        return payable + _shipping.For(payable);
    }

    public string DescribePassenger(Passenger c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == PassengerKind.Vip) s += " [VIP]";
        if (c.TripCount > PricingRules.LoyalTrips) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
