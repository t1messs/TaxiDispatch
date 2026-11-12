using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

public sealed record TotalRequest(
    int DocumentId,
    ClientInfo Client,
    IReadOnlyList<RouteSegment> Segments,
    DateOnly CreatedAt,
    string? CouponCode,
    DocumentStatus Status,
    decimal DeliveryPrice);
