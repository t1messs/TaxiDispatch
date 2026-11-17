namespace TaxiDispatch.Domain.Orders;

/// <summary>
/// Знімок стану замовлення для збереження у файл і відновлення з нього.
/// Не містить поведінки; цілісність перевіряє <see cref="TaxiOrder.Restore"/>.
/// </summary>
public sealed record OrderSnapshot
{
    /// <summary>Отримує номер замовлення.</summary>
    public required int Id { get; init; }

    /// <summary>Отримує номер пасажира.</summary>
    public required int PassengerId { get; init; }

    /// <summary>Отримує план поїздки.</summary>
    public required TripPlan Plan { get; init; }

    /// <summary>Отримує час створення.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Отримує стан замовлення.</summary>
    public required OrderStatus Status { get; init; }

    /// <summary>Отримує призначення водія, якщо воно було.</summary>
    public DriverAssignment? Assignment { get; init; }

    /// <summary>Отримує час посадки, якщо поїздка почалася.</summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>Отримує підсумок поїздки, якщо її завершено.</summary>
    public TripResult? Result { get; init; }

    /// <summary>Отримує скасування, якщо замовлення скасовано.</summary>
    public Cancellation? Cancellation { get; init; }
}
