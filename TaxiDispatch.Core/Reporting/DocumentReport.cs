using System;
using System.Globalization;
using System.Text;
using TaxiDispatch.Core.Documents;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Reporting;

public static class DocumentReport
{
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);

        // Фіксуємо культуру, щоб завжди була крапка у дробах (незалежно від Windows/Mac)
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();

        text.AppendLine($"Документ № {request.DocumentId}");
        text.AppendLine($"Клієнт: {request.Client.Name}");

        foreach (RouteSegment segment in request.Segments)
        {
            text.AppendLine($"{segment.Destination} x {segment.Distance}");
        }

        text.AppendLine(string.Format(culture, "Разом: {0:F2}", result.Total));
        text.AppendLine($"Стан: {result.Status}");

        return text.ToString();
    }
}
