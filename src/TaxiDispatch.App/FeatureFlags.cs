using System.Globalization;

namespace TaxiDispatch.App;

/// <summary>
/// Прапорці функцій (trunk-based development): незавершена функція вже злита в main,
/// але вимкнена, доки оператор не задасть змінну середовища.
/// </summary>
internal sealed class FeatureFlags
{
    /// <summary>Змінна середовища з коефіцієнтом попиту, наприклад «1.5».</summary>
    public const string SurgeVariable = "TAXI_SURGE_FACTOR";

    private FeatureFlags(decimal? surgeFactor) => SurgeFactor = surgeFactor;

    /// <summary>Отримує коефіцієнт попиту або <c>null</c>, якщо функцію вимкнено.</summary>
    public decimal? SurgeFactor { get; }

    /// <summary>Читає прапорці зі змінних середовища.</summary>
    /// <returns>Прапорці; без змінної функція вимкнена.</returns>
    public static FeatureFlags FromEnvironment()
    {
        string? text = Environment.GetEnvironmentVariable(SurgeVariable);
        bool isOn = decimal.TryParse(
            text,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal factor);
        return new FeatureFlags(isOn ? factor : null);
    }
}
