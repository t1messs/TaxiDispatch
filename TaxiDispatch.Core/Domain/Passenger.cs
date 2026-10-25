namespace TaxiDispatch.Core.Domain;

/// <summary>Пасажир, який замовляє таксі.</summary>
public sealed class Passenger
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
