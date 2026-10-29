using System;
using System.Collections.Generic;
using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

public static class DocumentGuards
{
    public static void EnsureHeaderValid(int documentId, string clientName, string clientEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientEmail);
        
        if (!clientEmail.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(clientEmail));
        }
    }

    public static void EnsureLineValid(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentException.ThrowIfNullOrWhiteSpace(segment.Destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(segment.Distance, nameof(segment.Distance));
        ArgumentOutOfRangeException.ThrowIfNegative(segment.PricePerKm, nameof(segment.PricePerKm));
    }

    public static void EnsureLinesValid(IReadOnlyList<RouteSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        if (segments.Count == 0)
        {
            throw new ArgumentException("Документ не містить жодної позиції.", nameof(segments));
        }
        foreach (RouteSegment segment in segments)
        {
            EnsureLineValid(segment);
        }
    }
}
