using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.App;

/// <summary>
/// Записує звіт про замовлення у файл.
/// </summary>
internal static class ReportFileExporter
{
    // БУЛО: при винятку файл лишається відкритим
    public static void Export(
        string path,
        IReadOnlyList<TaxiOrder> orders,
        Func<TaxiOrder, decimal> totalOf)
    {
        var writer = new StreamWriter(path);
        writer.WriteLine("id;total;status");
        foreach (TaxiOrder order in orders)
        {
            writer.WriteLine(order.Id + ";" + totalOf(order) + ";" + order.Status); // може кинути
        }

        writer.Close();
    }
}
