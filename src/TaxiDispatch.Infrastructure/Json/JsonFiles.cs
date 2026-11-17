using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace TaxiDispatch.Infrastructure.Json;

/// <summary>
/// Читання й запис файлів JSON. Пошкоджений файл або недопустимі дані в ньому
/// перетворюються на <see cref="InvalidDataException"/> з іменем файлу.
/// </summary>
internal static class JsonFiles
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Читає файл і перетворює вміст; обидва кроки — під одним захистом.</summary>
    /// <typeparam name="TFile">Тип вмісту файлу, наприклад список записів.</typeparam>
    /// <typeparam name="TResult">Тип результату після перетворення.</typeparam>
    /// <param name="path">Шлях до файлу.</param>
    /// <param name="map">Перетворення вмісту на об'єкти домену.</param>
    /// <returns>Перетворений вміст файлу.</returns>
    public static TResult Read<TFile, TResult>(string path, Func<TFile, TResult> map)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            TFile content = JsonSerializer.Deserialize<TFile>(stream, _options)
                ?? throw new InvalidDataException($"Файл {path} порожній.");
            return map(content);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Файл {path} пошкоджено: {error.Message}", error);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException(
                $"Файл {path}: недопустимі дані — {error.Message}",
                error);
        }
    }

    /// <summary>Записує значення через тимчасовий файл, щоб збій не лишив половину файлу.</summary>
    /// <typeparam name="T">Тип значення.</typeparam>
    /// <param name="path">Шлях до файлу.</param>
    /// <param name="value">Значення для запису.</param>
    public static void Write<T>(string path, T value)
    {
        string temporary = path + ".tmp";
        using (FileStream stream = File.Create(temporary))
        {
            JsonSerializer.Serialize(stream, value, _options);
        }

        File.Move(temporary, path, overwrite: true);
    }
}
