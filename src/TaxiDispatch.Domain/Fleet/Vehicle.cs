namespace TaxiDispatch.Domain.Fleet;

/// <summary>
/// Автомобіль водія — незмінний запис довідника автопарку.
/// </summary>
public sealed record Vehicle
{
    /// <summary>
    /// Створює автомобіль і перевіряє обов'язкові поля.
    /// </summary>
    /// <param name="plate">Номерний знак, не порожній, наприклад «AA1234BC».</param>
    /// <param name="model">Марка й модель, не порожні.</param>
    /// <param name="carClass">Клас автомобіля, за яким добирають замовлення.</param>
    /// <exception cref="ArgumentException">Номер або модель порожні.</exception>
    public Vehicle(string plate, string model, CarClass carClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plate);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        Plate = plate;
        Model = model;
        CarClass = carClass;
    }

    /// <summary>Отримує номерний знак.</summary>
    public string Plate { get; }

    /// <summary>Отримує марку й модель.</summary>
    public string Model { get; }

    /// <summary>Отримує клас автомобіля.</summary>
    public CarClass CarClass { get; }
}
