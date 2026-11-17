using TaxiDispatch.Domain.Pricing;
using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Звіт «Поїздки за зміну» (UC-06): рядки поїздок, підсумки водіїв і загальні числа.
/// </summary>
public sealed class ShiftReport
{
    /// <summary>
    /// Створює звіт.
    /// </summary>
    /// <param name="shift">Зміна, за яку побудовано звіт.</param>
    /// <param name="rows">Поїздки за часом завершення.</param>
    /// <param name="drivers">Підсумки водіїв за спаданням виручки.</param>
    /// <param name="cancelledCount">Кількість скасувань у межах зміни.</param>
    public ShiftReport(
        Shift shift,
        IReadOnlyList<ShiftReportRow> rows,
        IReadOnlyList<DriverTotal> drivers,
        int cancelledCount)
    {
        ArgumentNullException.ThrowIfNull(shift);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(drivers);

        Shift = shift;
        Rows = rows;
        Drivers = drivers;
        CancelledCount = cancelledCount;
    }

    /// <summary>Отримує зміну.</summary>
    public Shift Shift { get; }

    /// <summary>Отримує поїздки за часом завершення.</summary>
    public IReadOnlyList<ShiftReportRow> Rows { get; }

    /// <summary>Отримує підсумки водіїв за спаданням виручки.</summary>
    public IReadOnlyList<DriverTotal> Drivers { get; }

    /// <summary>Отримує кількість скасувань.</summary>
    public int CancelledCount { get; }

    /// <summary>Отримує сумарний пробіг, км.</summary>
    public decimal TotalKm => Rows.Sum(row => row.Km);

    /// <summary>Отримує виручку зміни, грн.</summary>
    public decimal Revenue => Rows.Sum(row => row.Fare);

    /// <summary>Отримує середній чек, грн; 0 для зміни без поїздок.</summary>
    public decimal AverageFare
    {
        get
        {
            if (Rows.Count == 0)
            {
                return 0m;
            }

            decimal average = Revenue / Rows.Count;
            return Math.Round(average, FareCalculator.MoneyDecimals, MidpointRounding.AwayFromZero);
        }
    }
}
