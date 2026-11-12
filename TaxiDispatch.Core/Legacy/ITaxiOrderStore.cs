namespace TaxiDispatch.Core.Legacy;

// Широкий інтерфейс з методички (лістинг 4.2): сім методів різного призначення.
public interface ITaxiOrderStore
{
    void Add(LegacyTaxiOrder order);

    LegacyTaxiOrder GetById(int id);

    void SaveToFile(LegacyTaxiOrder order, string path);

    void SendEmail(string to, string body);

    string ExportCsv();

    void Backup(string folder);

    void PrintInvoice(LegacyTaxiOrder order);
}
