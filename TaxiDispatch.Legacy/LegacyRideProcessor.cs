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
        _tmpDiscount = DiscountOf(_tmpSum, passengerKind, tripCount);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + rideId + "\n";
        txt += "Клієнт: " + passengerName + "\n";
        for (int i = 0; i < lines.Count; i++)
        {
            txt += lines[i].Code + " x" + lines[i].Km + " = " +
                (lines[i].Km * lines[i].PricePerKm)
                    .ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";

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

    private static decimal DiscountOf(decimal subtotal, PassengerKind kind, int tripCount)
    {
        decimal discount = 0m;
        if (kind == PassengerKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
        }
        else if (kind == PassengerKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
        }
        else if (tripCount > PricingRules.LoyalTrips)
        {
            discount = subtotal * PricingRules.LoyalRate;
        }

        // Обмеження знижки винесено з трьох гілок в одне місце: без знижки discount = 0 < 500.
        if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        return discount;
    }

    private static decimal ShippingOf(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;

    private static bool IsProcessable(RideState state) =>
        state == RideState.New || state == RideState.Paid;

    private static bool HasValidEmail(string? email) =>
        email != null && email.Contains("@");

    public decimal Preview(PassengerKind passengerKind, int tripCount,
        List<RideLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal payable = subtotal - DiscountOf(subtotal, passengerKind, tripCount);
        return payable + ShippingOf(payable);
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
