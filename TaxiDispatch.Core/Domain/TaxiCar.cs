namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Автомобіль таксі.
/// </summary>
public class TaxiCar
{
    /// <summary>Отримує або задає номерний знак.</summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>Отримує або задає модель автомобіля.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Отримує або задає категорію автомобіля (економ, комфорт тощо).</summary>
    public string Category { get; set; } = string.Empty;
}
