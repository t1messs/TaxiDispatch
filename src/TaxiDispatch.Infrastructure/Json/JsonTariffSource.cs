using TaxiDispatch.Domain.Pricing;

namespace TaxiDispatch.Infrastructure.Json;

/// <summary>
/// Тарифна сітка з файлу <c>tariffs.json</c>: новий тариф — рядок у файлі, а не зміна коду.
/// </summary>
public static class JsonTariffSource
{
    /// <summary>Ім'я файлу тарифів.</summary>
    public const string FileName = "tariffs.json";

    /// <summary>Завантажує тарифну сітку.</summary>
    /// <param name="paths">Розташування даних.</param>
    /// <returns>Тарифи всіх класів автомобілів.</returns>
    /// <exception cref="InvalidDataException">Файл пошкоджено або бракує тарифу.</exception>
    public static TariffBook Load(DataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return JsonFiles.Read<List<Tariff>, TariffBook>(
            paths.ForReading(FileName),
            tariffs => new TariffBook(tariffs));
    }
}
