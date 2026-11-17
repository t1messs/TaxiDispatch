namespace TaxiDispatch.Domain.Pricing;

/// <summary>
/// Вартість поїздки за правилом 4.1 специфікації. Чисті функції без побічних ефектів.
/// </summary>
public static class FareCalculator
{
    /// <summary>Множник вартості для посадки вночі.</summary>
    public const decimal NightSurcharge = 1.25m;

    /// <summary>Знаків після коми у грошових сумах: до копійки.</summary>
    public const int MoneyDecimals = 2;

    /// <summary>Година початку нічного тарифу (включно).</summary>
    public const int NightStartHour = 22;

    /// <summary>Година закінчення нічного тарифу (не включно).</summary>
    public const int NightEndHour = 6;

    /// <summary>
    /// Обчислює вартість: max(мінімум, посадка + ціна за км × км), уночі — помножену
    /// на <see cref="NightSurcharge"/>; округлення до копійок від нуля.
    /// </summary>
    /// <param name="tariff">Тариф класу автомобіля.</param>
    /// <param name="distanceKm">Пробіг, км; не від'ємний.</param>
    /// <param name="isNight">Чи припадає посадка на нічний час.</param>
    /// <returns>Вартість поїздки, грн.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Пробіг від'ємний.</exception>
    public static decimal Calculate(Tariff tariff, decimal distanceKm, bool isNight)
    {
        ArgumentNullException.ThrowIfNull(tariff);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceKm);

        decimal fare = Math.Max(tariff.Minimum, tariff.Boarding + (tariff.PerKm * distanceKm));
        if (isNight)
        {
            fare *= NightSurcharge;
        }

        return Math.Round(fare, MoneyDecimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Перевіряє, чи діє нічний тариф: з 22:00 включно до 06:00 не включно.
    /// </summary>
    /// <param name="time">Місцевий час посадки.</param>
    /// <returns><c>true</c>, якщо час нічний.</returns>
    public static bool IsNight(TimeOnly time) =>
        time.Hour >= NightStartHour || time.Hour < NightEndHour;
}
