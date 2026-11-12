namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Пасажир — незмінний запис довідника учасників.
/// </summary>
public sealed record Passenger
{
    private const string PhonePrefix = "+380";
    private const int PhoneLength = 13;

    /// <summary>
    /// Створює пасажира та перевіряє його інваріант.
    /// </summary>
    /// <param name="id">Ідентифікатор пасажира, більший за нуль.</param>
    /// <param name="name">Ім'я пасажира, не порожнє.</param>
    /// <param name="phoneNumber">Телефон у форматі +380XXXXXXXXX.</param>
    public Passenger(int id, string name, string phoneNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        if (!phoneNumber.StartsWith(PhonePrefix, StringComparison.Ordinal)
            || phoneNumber.Length != PhoneLength)
        {
            throw new ArgumentException(
                "Телефон має бути у форматі +380XXXXXXXXX.",
                nameof(phoneNumber));
        }

        Id = id;
        Name = name;
        PhoneNumber = phoneNumber;
    }

    /// <summary>Отримує ідентифікатор пасажира.</summary>
    public int Id { get; }

    /// <summary>Отримує ім'я пасажира.</summary>
    public string Name { get; }

    /// <summary>Отримує номер телефону пасажира.</summary>
    public string PhoneNumber { get; }

    /// <summary>Отримує ознаку постійного пасажира; задається лише при створенні.</summary>
    public bool IsRegular { get; init; }
}
