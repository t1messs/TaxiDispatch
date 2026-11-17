using TaxiDispatch.Core.Legacy;

namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Перевірки вхідних даних модуля документів: порушення — виняток аргументу.
/// </summary>
public static class DocumentGuards
{
    /// <summary>Перевіряє заголовок документа.</summary>
    /// <param name="documentId">Номер документа.</param>
    /// <param name="clientName">Ім'я пасажира.</param>
    /// <param name="clientEmail">Пошта пасажира.</param>
    /// <exception cref="ArgumentException">Порожнє ім'я або пошта без символу @.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Номер документа не додатний.</exception>
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

    /// <summary>Перевіряє весь запит на розрахунок.</summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <exception cref="ArgumentException">Порушено будь-яку перевірку запиту.</exception>
    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Client);
        EnsureHeaderValid(request.DocumentId, request.Client.Name, request.Client.Email);
        EnsureLinesValid(request.Segments);
        ArgumentOutOfRangeException.ThrowIfNegative(request.DeliveryPrice);
    }

    /// <summary>Перевіряє одну позицію документа.</summary>
    /// <param name="segment">Відрізок маршруту.</param>
    /// <exception cref="ArgumentException">
    /// Порожня адреса або недопустима відстань чи ціна.
    /// </exception>
    public static void EnsureLineValid(RouteSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentException.ThrowIfNullOrWhiteSpace(segment.Destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            segment.Distance,
            nameof(segment.Distance));
        ArgumentOutOfRangeException.ThrowIfNegative(
            segment.PricePerKm,
            nameof(segment.PricePerKm));
    }

    /// <summary>Перевіряє, що документ має хоча б одну коректну позицію.</summary>
    /// <param name="segments">Позиції документа.</param>
    /// <exception cref="ArgumentException">Позицій немає або одна з них некоректна.</exception>
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
