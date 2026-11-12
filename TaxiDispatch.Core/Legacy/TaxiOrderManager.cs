namespace TaxiDispatch.Core.Legacy;

// Успадкований «клас-Бог» з методички (ЛР4, лістинг 4.1) для аудиту SOLID.
// Порушення навмисні: їх виправлено в нових типах (завдання 6).
public class TaxiOrderManager
{
    private readonly FileOrderStore _store = new FileOrderStore();
    private readonly SmtpMailer _mailer = new SmtpMailer();

    public decimal Place(LegacyTaxiOrder order, string customerType)
    {
        decimal total = 0m;
        foreach (var line in order.Lines)
        {
            total += line.PricePerKm * line.Distance;
        }

        if (customerType == "regular")
        {
            total = total * 0.95m;
        }
        else if (customerType == "vip")
        {
            total = total * 0.90m;
        }
        else if (customerType == "staff")
        {
            total = total * 0.70m;
        }

        if (total > 1000m)
        {
            total = total - 50m;
        }

        order.Total = total;
        order.Status = 1;
        _store.SaveToFile(order, "orders.json");
        _mailer.Send(order.Email, "Order " + order.Id, "Сума: " + total);
        return total;
    }

    public string BuildCsvReport(List<LegacyTaxiOrder> orders)
    {
        var text = "id;total;status" + Environment.NewLine;
        foreach (var order in orders)
        {
            text += order.Id + ";" + order.Total + ";" + order.Status;
        }

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    public void Archive(LegacyTaxiOrder order)
    {
        LegacyTaxiOrder archived = new ArchivedTaxiOrder(order.Id);
        foreach (var line in order.Lines)
        {
            archived.AddLine(line);
        }

        _store.SaveToFile(archived, "archive.json");
    }
}
