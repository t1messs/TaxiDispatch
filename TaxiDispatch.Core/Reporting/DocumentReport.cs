using System.Globalization;
using System.Text;
using TaxiDispatch.Core.Documents;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Reporting;

/// <summary>
/// Текстовий звіт про розрахований документ.
/// </summary>
public static class DocumentReport
{
    /// <summary>Формує текст звіту з фіксованою культурою. Не має побічних ефектів.</summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Багаторядковий текст звіту.</returns>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);

        // Фіксуємо культуру, щоб завжди була крапка у дробах (незалежно від Windows/Mac)
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();

        text.AppendLine(culture, $"Документ № {request.DocumentId}");
        text.AppendLine(culture, $"Клієнт: {request.Client.Name}");

        foreach (RouteSegment segment in request.Segments)
        {
            text.AppendLine(culture, $"{segment.Destination} x {segment.Distance}");
        }

        text.AppendLine(string.Format(culture, "Разом: {0:F2}", result.Total));
        text.AppendLine(culture, $"Стан: {result.Status}");

        return text.ToString();
    }
}
