using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Application.Orders;

/// <summary>
/// Перевірені барикадою дані нового замовлення (UC-01).
/// </summary>
public sealed record OrderRequest
{
    /// <summary>
    /// Створює запит на замовлення.
    /// </summary>
    /// <param name="phone">Телефон пасажира, уже перевірений барикадою.</param>
    /// <param name="passengerName">Ім'я нового пасажира; <c>null</c>, якщо пасажир відомий.</param>
    /// <param name="plan">Звідки, куди й яким класом.</param>
    public OrderRequest(string phone, string? passengerName, TripPlan plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentNullException.ThrowIfNull(plan);

        Phone = phone;
        PassengerName = passengerName;
        Plan = plan;
    }

    /// <summary>Отримує телефон пасажира.</summary>
    public string Phone { get; }

    /// <summary>Отримує ім'я нового пасажира або <c>null</c>.</summary>
    public string? PassengerName { get; }

    /// <summary>Отримує план поїздки.</summary>
    public TripPlan Plan { get; }
}
