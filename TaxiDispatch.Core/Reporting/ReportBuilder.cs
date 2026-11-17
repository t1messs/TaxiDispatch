using System.Text;

namespace TaxiDispatch.Core.Reporting;

/// <summary>
/// Формує текст простого звіту з готових рядків.
/// </summary>
public sealed class ReportBuilder
{
    /// <summary>Отримує або задає заголовок звіту.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>
    /// Будує звіт: заголовок великими літерами, далі рядки. Не має побічних ефектів.
    /// </summary>
    /// <param name="rows">Рядки звіту.</param>
    /// <returns>Текст звіту.</returns>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var text = new StringBuilder();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (var row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }
}
