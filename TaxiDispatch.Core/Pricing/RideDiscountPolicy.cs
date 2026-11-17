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
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Вартість поїздки не може бути від'ємною.");
        }

        decimal rate = 0m;
        if (isRegular)
        {
            rate += RegularRate;
        }

        if (amount >= LongRideFrom)
        {
            rate += LongRideRate;
        }

        decimal discount = decimal.Round(amount * rate, 2);
        if (discount > MaxDiscount)
        {
            discount = MaxDiscount;
        }

        return discount;
    }
}
