using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Application.Queries;

/// <summary>
/// Критерії пошуку замовлень (UC-05); незадані критерії — <c>null</c>.
/// </summary>
public sealed record OrderSearchCriteria
{
    /// <summary>Отримує потрібний стан замовлення.</summary>
    public OrderStatus? Status { get; init; }

    /// <summary>Отримує дату створення за місцевим часом.</summary>
    public DateOnly? CreatedOn { get; init; }

    /// <summary>Отримує телефон пасажира, перевірений барикадою.</summary>
    public string? Phone { get; init; }

    /// <summary>Отримує ознаку, що жодного критерію не задано.</summary>
    public bool IsEmpty => Status is null && CreatedOn is null && Phone is null;
}
