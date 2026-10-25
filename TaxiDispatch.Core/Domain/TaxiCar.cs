namespace TaxiDispatch.Core.Domain;

/// <summary>Автомобіль таксі.</summary>
public sealed class TaxiCar
{
    public string LicensePlate { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}
