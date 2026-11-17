namespace TaxiDispatch.Application.Validation;

/// <summary>
/// Результат операції для очікуваних помилок користувача: або значення, або пояснення.
/// </summary>
/// <typeparam name="T">Тип значення в разі успіху.</typeparam>
public readonly struct Result<T>
{
    internal Result(bool ok, T? value, string error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    /// <summary>Отримує ознаку успіху.</summary>
    public bool IsSuccess { get; }

    /// <summary>Отримує значення; має сенс лише в разі успіху.</summary>
    public T? Value { get; }

    /// <summary>Отримує пояснення помилки; порожнє в разі успіху.</summary>
    public string Error { get; }
}

/// <summary>
/// Фабрика результатів. Статичні методи винесено з generic-типу (правило CA1000),
/// тому виклик коротший: Result.Ok(value) замість Result&lt;T&gt;.Ok(value).
/// </summary>
public static class Result
{
    /// <summary>Створює успішний результат.</summary>
    /// <typeparam name="T">Тип значення.</typeparam>
    /// <param name="value">Отримане значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok<T>(T value) => new(true, value, string.Empty);

    /// <summary>Створює результат із поясненням помилки.</summary>
    /// <typeparam name="T">Тип значення, яке не вдалося отримати.</typeparam>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Неуспішний результат.</returns>
    public static Result<T> Fail<T>(string error) => new(false, default, error);
}
