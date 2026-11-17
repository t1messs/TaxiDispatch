using System.Diagnostics.CodeAnalysis;

namespace TaxiDispatch.Core.Pricing;

/// <summary>
/// Знижка на поїздку: постійному пасажиру та за дорогу (довгу) поїздку.
/// </summary>
public sealed class RideDiscountPolicy
{
    /// <summary>Знижка постійному пасажиру.</summary>
    public const decimal RegularRate = 0.05m;

    /// <summary>Додаткова знижка за дорогу поїздку.</summary>
    public const decimal LongRideRate = 0.10m;

    /// <summary>Межа дорогої поїздки, грн.</summary>
    public const decimal LongRideFrom = 1000m;

    /// <summary>Максимальна сума знижки, грн.</summary>
    public const decimal MaxDiscount = 150m;

    /// <summary>
    /// Обчислює знижку на поїздку.
    /// </summary>
    /// <param name="amount">Вартість поїздки, грн; не може бути від'ємною.</param>
    /// <param name="isRegular">Ознака постійного пасажира.</param>
    /// <returns>Сума знижки, заокруглена до копійок.</returns>
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Політика — об'єкт, який передають залежністю; правила можуть змінитися.")]
    public decimal DiscountFor(decimal amount, bool isRegular)
    {
        EnsureAmountIsValid(amount);
        decimal raw = amount * RateFor(amount, isRegular);
        return Math.Min(decimal.Round(raw, 2), MaxDiscount);
    }

    private static void EnsureAmountIsValid(decimal amount)
    {
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Вартість поїздки не може бути від'ємною.");
        }
    }

    private static decimal RateFor(decimal amount, bool isRegular)
    {
        decimal rate = isRegular ? RegularRate : 0m;
        if (amount >= LongRideFrom)
        {
            rate += LongRideRate;
        }

        return rate;
    }
}
