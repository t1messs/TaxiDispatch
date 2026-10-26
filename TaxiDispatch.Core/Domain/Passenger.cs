namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Пасажир таксі.
/// </summary>
public class Passenger
{
    /// <summary>Отримує або задає ідентифікатор пасажира.</summary>
    public int Id { get; set; }

    /// <summary>Отримує або задає ім'я пасажира.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Отримує або задає номер телефону.</summary>
    public string PhoneNumber { get; set; } = string.Empty;
}