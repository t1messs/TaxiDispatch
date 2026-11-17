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

    // Перенесено з LegacyRideProcessor.DescribePassenger: метод читав лише поля Passenger.
    public string Describe()
    {
        string s = Name.Trim().ToUpper();
        if (Kind == PassengerKind.Vip)
        {
            s += " [VIP]";
        }

        if (TripCount > PricingRules.LoyalTrips)
        {
            s += " [ЛОЯЛЬНИЙ]";
        }

        s += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }
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
    private readonly List<string> _log = new();
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();

    public string Handle(RideRequest req)
    {
        IReadOnlyList<RideLine>? lines = req.Lines;

        // Охоронні умови: порядок перевірок той самий, що й у вкладених if.
        if (lines == null)
        {
            return "ERR: null";
        }

        if (lines.Count == 0)
        {
            return "ERR: empty";
        }

        if (!IsProcessable(req.State))
        {
            return "ERR: state";
        }

        if (!HasValidEmail(req.Passenger.Email))
        {
            return "ERR: mail";
        }

        _log.Add("ok " + req.RideId);

        decimal subtotal = Subtotal(lines);
        decimal discount = _discounts.For(subtotal, req.Passenger.Kind, req.Passenger.TripCount);
        decimal ship = _shipping.For(subtotal - discount);
        decimal total = subtotal - discount + ship;

        string txt = _report.Build(req, discount, ship, total);

        if (req.SendMail)
        {
            _log.Add("mail -> " + req.Passenger.Email);
        }
        return txt;
    }

    private static decimal Subtotal(IReadOnlyList<RideLine> lines)
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

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log)
        {
            r += l + "\n";
        }

        return r;
    }
}
