using System.Globalization;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;

namespace TaxiDispatch.App.Input;

/// <summary>
/// Барикада: перетворює сирі рядки з консолі на перевірений відрізок маршруту.
/// </summary>
internal static class RouteSegmentParser
{
    // Застосунок очікує десяткову кому (культура uk-UA); крапку теж приймаємо, замінюючи на кому.
    private static readonly CultureInfo _inputCulture = CultureInfo.GetCultureInfo("uk-UA");

    /// <summary>
    /// Розбирає адресу, дистанцію й ціну за кілометр.
    /// </summary>
    /// <param name="destination">Адреса призначення.</param>
    /// <param name="distanceKm">Дистанція в кілометрах (рядок).</param>
    /// <param name="pricePerKm">Ціна за кілометр (рядок).</param>
    /// <returns>Готовий відрізок або пояснення помилки.</returns>
    public static Result<RouteSegment> Parse(
        string? destination,
        string? distanceKm,
        string? pricePerKm)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return Result.Fail<RouteSegment>("Адреса призначення не може бути порожньою");
        }

        if (!TryParseNumber(distanceKm, out decimal distance) || distance <= 0)
        {
            return Result.Fail<RouteSegment>("Дистанція — число більше 0, наприклад 3,5");
        }

        if (!TryParseNumber(pricePerKm, out decimal price) || price < 0)
        {
            return Result.Fail<RouteSegment>("Ціна за кілометр — число не менше 0");
        }

        return Result.Ok(new RouteSegment(destination.Trim(), distance, price));
    }

    /// <summary>
    /// Рахує рядки, що є коректною дистанцією (число більше 0); умова замість try/catch.
    /// </summary>
    /// <param name="rows">Сирі рядки з введення.</param>
    /// <returns>Кількість коректних дистанцій.</returns>
    public static int CountValidDistances(IEnumerable<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        int valid = 0;
        foreach (string row in rows)
        {
            if (TryParseNumber(row, out decimal km) && km > 0)
            {
                valid++;
            }
        }

        return valid;
    }

    private static bool TryParseNumber(string? text, out decimal value)
    {
        string normalized = (text ?? string.Empty).Trim().Replace('.', ',');
        return decimal.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint,
            _inputCulture,
            out value);
    }
}
