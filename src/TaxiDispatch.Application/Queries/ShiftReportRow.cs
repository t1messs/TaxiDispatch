using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Рядок звіту «Поїздки за зміну»: одна завершена поїздка.
/// </summary>
public sealed record ShiftReportRow
{
    /// <summary>Отримує номер замовлення.</summary>
    public required int OrderId { get; init; }

    /// <summary>Отримує час завершення поїздки.</summary>
    public required DateTimeOffset CompletedAt { get; init; }

    /// <summary>Отримує номер водія.</summary>
    public required int DriverId { get; init; }

    /// <summary>Отримує ім'я водія.</summary>
    public required string DriverName { get; init; }

    /// <summary>Отримує клас автомобіля.</summary>
    public required CarClass CarClass { get; init; }

    /// <summary>Отримує пробіг, км.</summary>
    public required decimal Km { get; init; }

    /// <summary>Отримує вартість, грн.</summary>
    public required decimal Fare { get; init; }
}
