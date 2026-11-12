namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Автомобіль таксі — змінний об'єкт обліку з контрольованими змінами.
/// </summary>
public sealed class TaxiCar
{
    /// <summary>
    /// Створює автомобіль, доступний для замовлень, з нульовим пробігом.
    /// </summary>
    /// <param name="licensePlate">Номерний знак, не порожній.</param>
    /// <param name="model">Модель автомобіля, не порожня.</param>
    /// <param name="category">Категорія (економ, комфорт тощо), не порожня.</param>
    public TaxiCar(string licensePlate, string model, string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licensePlate);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        LicensePlate = licensePlate;
        Model = model;
        Category = category;
        IsAvailable = true;
    }

    /// <summary>Отримує номерний знак.</summary>
    public string LicensePlate { get; }

    /// <summary>Отримує модель автомобіля.</summary>
    public string Model { get; }

    /// <summary>Отримує категорію автомобіля.</summary>
    public string Category { get; }

    /// <summary>Отримує пробіг у кілометрах; лише зростає.</summary>
    public decimal MileageKm { get; private set; }

    /// <summary>Отримує ознаку, чи вільний автомобіль для нового замовлення.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>
    /// Додає пробіг після поїздки; від'ємне чи нульове значення заборонене.
    /// </summary>
    /// <param name="distanceKm">Пройдена дистанція в кілометрах.</param>
    public void AddMileage(decimal distanceKm)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(distanceKm);
        MileageKm += distanceKm;
    }

    /// <summary>Позначає автомобіль зайнятим.</summary>
    public void MarkBusy() => IsAvailable = false;

    /// <summary>Позначає автомобіль вільним.</summary>
    public void MarkAvailable() => IsAvailable = true;
}
