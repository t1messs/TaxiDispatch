using System.Globalization;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.App;

/// <summary>
/// Записує звіт про замовлення у файл.
/// </summary>
internal static class ReportFileExporter
{
    /// <summary>
    /// Зберігає CSV-звіт; using звільняє потік навіть тоді, коли під час запису кинуто виняток.
    /// </summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="orders">Замовлення для звіту.</param>
    /// <param name="totalOf">Функція, що повертає суму замовлення.</param>
    public static void Export(
        string path,
        IReadOnlyList<TaxiOrder> orders,
        Func<TaxiOrder, decimal> totalOf)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(totalOf);

        // СТАЛО: using звільняє потік навіть при винятку
        using var writer = new StreamWriter(path);
        writer.WriteLine("id;total;status");
        foreach (TaxiOrder order in orders)
        {
            writer.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{order.Id};{totalOf(order):0.00};{order.Status}"));
        }
    }
}
