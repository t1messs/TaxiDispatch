using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Вхідні дані для розрахунку документа (об'єкт-параметр замість семи аргументів).
/// </summary>
/// <param name="DocumentId">Номер документа, більший за нуль.</param>
/// <param name="Client">Дані пасажира.</param>
/// <param name="Segments">Позиції документа — відрізки маршруту.</param>
/// <param name="CreatedAt">Дата створення документа.</param>
/// <param name="CouponCode">Код купона або <c>null</c>.</param>
/// <param name="Status">Поточний стан документа.</param>
/// <param name="DeliveryPrice">Вартість подачі авто, грн.</param>
public sealed record TotalRequest(
    int DocumentId,
    ClientInfo Client,
    IReadOnlyList<RouteSegment> Segments,
    DateOnly CreatedAt,
    string? CouponCode,
    DocumentStatus Status,
    decimal DeliveryPrice);
