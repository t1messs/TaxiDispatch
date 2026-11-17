using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TaxiDispatch.Domain.Geo;

/// <summary>
/// Географічна точка в системі координат WGS-84: широта й довгота в градусах.
/// </summary>
public sealed record GeoPoint
{
    /// <summary>Найбільша за модулем широта, градусів.</summary>
    public const double MaxLatitude = 90.0;

    /// <summary>Найбільша за модулем довгота, градусів.</summary>
    public const double MaxLongitude = 180.0;

    /// <summary>
    /// Створює точку й перевіряє межі координат (правило X-03).
    /// </summary>
    /// <param name="latitude">Широта, градусів: від −90 до 90, додатна — північна.</param>
    /// <param name="longitude">Довгота, градусів: від −180 до 180, додатна — східна.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Координата поза межами або не є скінченним числом.
    /// </exception>
    public GeoPoint(double latitude, double longitude)
    {
        EnsureInRange(latitude, MaxLatitude, nameof(latitude));
        EnsureInRange(longitude, MaxLongitude, nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Отримує широту, градусів.</summary>
    public double Latitude { get; }

    /// <summary>Отримує довготу, градусів.</summary>
    public double Longitude { get; }

    /// <summary>
    /// Розбирає координати, введені людиною: «широта; довгота», дробова частина — через кому
    /// або крапку, наприклад «50,4501; 30,5234». Не має побічних ефектів.
    /// </summary>
    /// <param name="text">Рядок із координатами.</param>
    /// <param name="point">Розібрана точка або <c>null</c>.</param>
    /// <returns><c>true</c>, якщо обидві координати — числа в допустимих межах.</returns>
    public static bool TryParse(string? text, [NotNullWhen(true)] out GeoPoint? point)
    {
        point = null;
        string[] parts = (text ?? string.Empty).Split(';', StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !TryParseDegrees(parts[0], MaxLatitude, out double latitude)
            || !TryParseDegrees(parts[1], MaxLongitude, out double longitude))
        {
            return false;
        }

        point = new GeoPoint(latitude, longitude);
        return true;
    }

    private static bool TryParseDegrees(string text, double maxAbsDegrees, out double degrees) =>
        double.TryParse(
            text.Replace(',', '.'),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out degrees)
        && double.IsFinite(degrees)
        && Math.Abs(degrees) <= maxAbsDegrees;

    private static void EnsureInRange(double degrees, double maxAbsDegrees, string paramName)
    {
        if (!double.IsFinite(degrees) || Math.Abs(degrees) > maxAbsDegrees)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                "Координата має бути скінченним числом у допустимих межах.");
        }
    }
}
