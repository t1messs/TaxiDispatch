using System.Diagnostics.CodeAnalysis;

namespace TaxiDispatch.Domain.Passengers;

/// <summary>
/// Пасажир — незмінний запис довідника; впізнається за телефоном.
/// </summary>
public sealed record Passenger
{
    /// <summary>Префікс українського мобільного номера.</summary>
    public const string PhonePrefix = "+380";

    /// <summary>Довжина номера разом із префіксом.</summary>
    public const int PhoneLength = 13;

    /// <summary>
    /// Створює пасажира й перевіряє його інваріант.
    /// </summary>
    /// <param name="id">Номер пасажира, більший за нуль.</param>
    /// <param name="name">Ім'я пасажира, не порожнє.</param>
    /// <param name="phone">Телефон у форматі +380XXXXXXXXX.</param>
    /// <exception cref="ArgumentException">Порушено інваріант пасажира.</exception>
    public Passenger(int id, string name, string phone)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!IsValidPhone(phone))
        {
            throw new ArgumentException("Телефон має бути у форматі +380XXXXXXXXX.", nameof(phone));
        }

        Id = id;
        Name = name;
        Phone = phone;
    }

    /// <summary>Отримує номер пасажира.</summary>
    public int Id { get; }

    /// <summary>Отримує ім'я пасажира.</summary>
    public string Name { get; }

    /// <summary>Отримує телефон пасажира.</summary>
    public string Phone { get; }

    /// <summary>
    /// Перевіряє формат телефону: +380 і дев'ять цифр. Не має побічних ефектів.
    /// </summary>
    /// <param name="phone">Рядок, введений користувачем або прочитаний із файлу.</param>
    /// <returns><c>true</c>, якщо формат правильний.</returns>
    public static bool IsValidPhone([NotNullWhen(true)] string? phone) =>
        phone is not null
        && phone.Length == PhoneLength
        && phone.StartsWith(PhonePrefix, StringComparison.Ordinal)
        && phone.Skip(1).All(char.IsAsciiDigit);
}
