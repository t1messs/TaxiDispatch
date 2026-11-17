namespace TaxiDispatch.Core.Reporting;

/// <summary>
/// Виводить готовий текст звіту в заданий потік.
/// </summary>
public sealed class DocumentReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>Створює принтер звітів.</summary>
    /// <param name="output">Потік виводу.</param>
    public DocumentReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>Виводить звіт у потік.</summary>
    /// <param name="report">Непорожній текст звіту.</param>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
