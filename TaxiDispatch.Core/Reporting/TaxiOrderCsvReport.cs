using System.Globalization;
using System.Text;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Reporting;

/// <summary>
/// Форматувальник CSV-звіту: лише будує рядок, на диск нічого не пише.
/// </summary>
public static class TaxiOrderCsvReport
{
    /// <summary>
    /// Формує CSV-звіт із заголовком і рядком на кожне замовлення.
    /// </summary>
    /// <param name="orders">Замовлення для звіту.</param>
    /// <param name="totalOf">Функція, що повертає суму замовлення.</param>
    /// <returns>Текст звіту у форматі CSV.</returns>
    public static string BuildCsv(IEnumerable<TaxiOrder> orders, Func<TaxiOrder, decimal> totalOf)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(totalOf);

        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine("id;total;status");
        foreach (TaxiOrder order in orders)
        {
            text.AppendLine(culture, $"{order.Id};{totalOf(order):0.00};{order.Status}");
        }

        return text.ToString();
    }
}
