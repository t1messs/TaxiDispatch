using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

public sealed record ClientInfo(
    string Name,
    string Email,
    bool IsRegular);

public sealed record TotalRequest(
    int DocumentId,
    ClientInfo Client,
    IReadOnlyList<RouteSegment> Segments,
    DateOnly CreatedAt,
    string? CouponCode,
    DocumentStatus Status,
    decimal DeliveryPrice);

public sealed record TotalResult(
    decimal Total,
    DocumentStatus Status);
