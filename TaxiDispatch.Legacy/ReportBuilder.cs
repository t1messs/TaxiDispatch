using System.Text;

namespace TaxiDispatch.Legacy;

public sealed class ReportBuilder
{
    public string Build(int rideId, string passengerName, List<RideLine> lines,
        string currency, decimal discount, decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(rideId).Append('\n');
        sb.Append("Клієнт: ").Append(passengerName).Append('\n');
        foreach (var line in lines)
        {
            sb.Append(Line(line, currency));
        }

        sb.Append(Money("Знижка", discount, currency));
        sb.Append(Money("Доставка", shipping, currency));
        sb.Append(Money("Разом", total, currency));
        return sb.ToString();
    }

    private static string Line(RideLine line, string currency) =>
        line.Code + " x" + line.Km + " = " +
        (line.Km * line.PricePerKm).ToString("0.00") + " " + currency + "\n";

    private static string Money(string label, decimal value, string currency) =>
        label + ": " + value.ToString("0.00") + " " + currency + "\n";
}
