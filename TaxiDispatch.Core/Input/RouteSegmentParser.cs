using System.Globalization;
using TaxiDispatch.Core.Domain;
using TaxiDispatch.Core.Errors;

namespace TaxiDispatch.Core.Input;

/// <summary>
/// Барикада: межа між «брудними» рядками ззовні та перевіреним відрізком маршруту.
/// </summary>
public static class RouteSegmentParser
{
    /// <summary>Найдовша допустима адреса, символів.</summary>
    public const int MaxDestinationLength = 100;

    /// <summary>Найбільша допустима дистанція одного відрізка, км.</summary>
    public const decimal MaxDistanceKm = 1000m;

    /// <summary>Найбільша допустима ціна за кілометр, грн.</summary>
    public const decimal MaxPricePerKm = 500m;

    // Застосунок очікує десяткову кому (культура uk-UA); крапку теж приймаємо, замінюючи на кому.
    private static readonly CultureInfo _inputCulture = CultureInfo.GetCultureInfo("uk-UA");

    /// <summary>
    /// Розбирає адресу, дистанцію й ціну за кілометр і перевіряє їхні межі.
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
        string address = destination?.Trim() ?? string.Empty;
        if (address.Length is 0 or > MaxDestinationLength)
        {
            return Result.Fail<RouteSegment>(
                $"Адреса призначення — від 1 до {MaxDestinationLength} символів");
        }

        if (!TryParseNumber(distanceKm, out decimal distance)
            || distance is <= 0m or > MaxDistanceKm)
        {
            return Result.Fail<RouteSegment>(
                "Дистанція — число більше 0 і не більше 1000 км, наприклад 3,5");
        }

        if (!TryParseNumber(pricePerKm, out decimal price)
            || price is < 0m or > MaxPricePerKm)
        {
            return Result.Fail<RouteSegment>(
                "Ціна за кілометр — число від 0 до 500 грн");
        }

        return Result.Ok(new RouteSegment(address, distance, price));
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
