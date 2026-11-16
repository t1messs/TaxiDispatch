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
        if (lines != null)
        {
            if (lines.Count > 0)
            {
                if (state == RideState.New || state == RideState.Paid)
                {
                    if (passengerEmail != null &&
                        passengerEmail.Contains("@"))
                    {
                        _log.Add("ok " + rideId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            _tmpSum += lines[i].Km * lines[i].PricePerKm;
        }

        _tmpDiscount = 0m;
        if (passengerKind == PassengerKind.Vip)
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (passengerKind == PassengerKind.Staff)
        {
            _tmpDiscount = _tmpSum * PricingRules.StaffRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (tripCount > PricingRules.LoyalTrips)
        {
            _tmpDiscount = _tmpSum * PricingRules.LoyalRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < PricingRules.FreeShipFrom) ship = PricingRules.ShipCost;
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

    public decimal Preview(PassengerKind passengerKind, int tripCount,
        List<RideLine> lines)
    {
        decimal s = 0m;
        foreach (var it in lines)
        {
            s += it.Km * it.PricePerKm;
        }
        decimal d = 0m;
        if (passengerKind == PassengerKind.Vip)
        {
            d = s * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (passengerKind == PassengerKind.Staff)
        {
            d = s * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (tripCount > PricingRules.LoyalTrips)
        {
            d = s * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        decimal sh = 0m;
        if (s - d < PricingRules.FreeShipFrom) sh = PricingRules.ShipCost;
        return s - d + sh;
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
