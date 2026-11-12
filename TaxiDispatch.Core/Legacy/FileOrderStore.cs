namespace TaxiDispatch.Core.Legacy;

// Навчальна заглушка: у методичці клас згадано, але не наведено.
// Змушена реалізовувати всі сім методів широкого інтерфейсу.
public class FileOrderStore : ITaxiOrderStore
{
    public void Add(LegacyTaxiOrder order) => throw new NotSupportedException();

    public LegacyTaxiOrder GetById(int id) => throw new NotSupportedException();

    public void SaveToFile(LegacyTaxiOrder order, string path)
    {
        File.WriteAllText(path, order.Id + ";" + order.Total + ";" + order.Status);
    }

    public void SendEmail(string to, string body) => throw new NotSupportedException();

    public string ExportCsv() => throw new NotSupportedException();

    public void Backup(string folder) => throw new NotSupportedException();

    public void PrintInvoice(LegacyTaxiOrder order) => throw new NotSupportedException();
}
