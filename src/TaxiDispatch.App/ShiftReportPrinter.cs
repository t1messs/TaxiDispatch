using TaxiDispatch.Application.Queries;
using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.App;

/// <summary>
/// Форматований вивід звіту «Поїздки за зміну» (UC-06) таблицею.
/// </summary>
internal static class ShiftReportPrinter
{
    public static void Print(ConsoleIo io, ShiftReport report)
    {
        Shift shift = report.Shift;
        string kind = shift.Kind == ShiftKind.Day ? "денна" : "нічна";
        io.Say(
            $"Звіт «Поїздки за зміну»: {kind}, "
            + $"{shift.Start:dd.MM.yyyy HH:mm} — {shift.End:dd.MM.yyyy HH:mm}");
        if (report.Rows.Count == 0)
        {
            io.Say("  Завершених поїздок у зміні немає.");
        }
        else
        {
            io.Say(
                $"  {"№",-5} {"Завершено",-9} {"Водій",-22} {"Клас",-9} {"Км",8} {"Вартість",10}");
            foreach (ShiftReportRow row in report.Rows)
            {
                io.Say(
                    $"  {row.OrderId,-5} {row.CompletedAt:HH:mm}     {row.DriverName,-22} "
                    + $"{row.CarClass,-9} {row.Km,8:0.000} {row.Fare,10:N2}");
            }
        }

        io.Say(
            $"Поїздок: {report.Rows.Count}; пробіг: {Formats.Km(report.TotalKm)}; "
            + $"виручка: {Formats.Money(report.Revenue)}; середній чек: "
            + $"{Formats.Money(report.AverageFare)}; скасувань: {report.CancelledCount}");
        foreach (DriverTotal total in report.Drivers)
        {
            string revenue = Formats.Money(total.Revenue);
            io.Say($"  {total.DriverName,-22} поїздок: {total.Rides,2}  {revenue,14}");
        }
    }
}
