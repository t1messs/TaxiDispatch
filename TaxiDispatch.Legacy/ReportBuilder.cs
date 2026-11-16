using System.Text;

namespace TaxiDispatch.Legacy;

public sealed class ReportBuilder
{
    public string Build(RideRequest req, decimal discount, decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(req.RideId).Append('\n');
        sb.Append("Клієнт: ").Append(req.Passenger.Name).Append('\n');

        // Lines уже перевірено на null у Handle до виклику Build.
        foreach (var line in req.Lines!)
        {
            sb.Append(Line(line, req.Currency));
        }

        sb.Append(Money("Знижка", discount, req.Currency));
        sb.Append(Money("Доставка", shipping, req.Currency));
        sb.Append(Money("Разом", total, req.Currency));
        return sb.ToString();
    }

    private static string Line(RideLine line, string currency) =>
        line.Code + " x" + line.Km + " = " +
        (line.Km * line.PricePerKm).ToString("0.00") + " " + currency + "\n";

    private static string Money(string label, decimal value, string currency) =>
        label + ": " + value.ToString("0.00") + " " + currency + "\n";
}
