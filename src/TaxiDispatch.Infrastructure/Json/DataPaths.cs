namespace TaxiDispatch.Infrastructure.Json;

/// <summary>
/// Розташування даних: незмінні умовні дані в <c>data/</c> і робочий стан у <c>state/</c>.
/// Застосунок читає файл зі стану, якщо він є, інакше — з умовних даних; пише лише в стан.
/// </summary>
public sealed record DataPaths
{
    /// <summary>
    /// Створює набір шляхів.
    /// </summary>
    /// <param name="seedDirectory">Тека з умовними даними з репозиторію.</param>
    /// <param name="stateDirectory">Тека робочого стану, яка в репозиторій не потрапляє.</param>
    public DataPaths(string seedDirectory, string stateDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(stateDirectory);

        SeedDirectory = seedDirectory;
        StateDirectory = stateDirectory;
    }

    /// <summary>Отримує теку з умовними даними.</summary>
    public string SeedDirectory { get; }

    /// <summary>Отримує теку робочого стану.</summary>
    public string StateDirectory { get; }

    /// <summary>Повертає шлях для читання: файл стану, якщо він є, інакше — з data/.</summary>
    /// <param name="fileName">Ім'я файлу, наприклад «orders.json».</param>
    /// <returns>Шлях до файлу.</returns>
    public string ForReading(string fileName)
    {
        string state = ForWriting(fileName);
        return File.Exists(state) ? state : Path.Combine(SeedDirectory, fileName);
    }

    /// <summary>Повертає шлях для запису в теку стану.</summary>
    /// <param name="fileName">Ім'я файлу.</param>
    /// <returns>Шлях до файлу в теці стану.</returns>
    public string ForWriting(string fileName) => Path.Combine(StateDirectory, fileName);
}
