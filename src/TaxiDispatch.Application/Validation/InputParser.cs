using System.Globalization;
using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Geo;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Domain.Places;
using TaxiDispatch.Domain.Shifts;

namespace TaxiDispatch.Application.Validation;

/// <summary>
/// Барикада вводу: єдине місце, де текст від користувача перетворюється на значення домену.
/// Помилка вводу — не виняток, а <see cref="Result{T}"/> з поясненням для повторного запиту.
/// </summary>
public static class InputParser
{
    /// <summary>Найбільший фактичний пробіг однієї поїздки, км.</summary>
    public const decimal MaxActualKm = 500m;

    /// <summary>Найбільша довжина імені чи причини скасування, символів.</summary>
    public const int MaxTextLength = 100;

    /// <summary>Код місця, заданого координатами, а не кодом довідника.</summary>
    public const string GpsCode = "GPS";

    private const string DateFormat = "dd.MM.yyyy";

    private static readonly Dictionary<string, CarClass> _carClasses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["1"] = CarClass.Economy,
            ["2"] = CarClass.Comfort,
            ["3"] = CarClass.Business,
            [nameof(CarClass.Economy)] = CarClass.Economy,
            [nameof(CarClass.Comfort)] = CarClass.Comfort,
            [nameof(CarClass.Business)] = CarClass.Business,
        };

    private static readonly Dictionary<string, ShiftKind> _shiftKinds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["д"] = ShiftKind.Day,
            ["н"] = ShiftKind.Night,
            [nameof(ShiftKind.Day)] = ShiftKind.Day,
            [nameof(ShiftKind.Night)] = ShiftKind.Night,
        };

    private static readonly Dictionary<string, OrderStatus> _statuses = Enum
        .GetNames<OrderStatus>()
        .ToDictionary(name => name, Enum.Parse<OrderStatus>, StringComparer.OrdinalIgnoreCase);

    /// <summary>Розбирає телефон: пробіли ігноруються, формат +380XXXXXXXXX.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Телефон або пояснення помилки.</returns>
    public static Result<string> ParsePhone(string? text)
    {
        string phone = Normalize(text).Replace(" ", string.Empty, StringComparison.Ordinal);
        return Passenger.IsValidPhone(phone)
            ? Result.Ok(phone)
            : Result.Fail<string>("Телефон у форматі +380XXXXXXXXX, наприклад +380501234567.");
    }

    /// <summary>Розбирає обов'язковий текст: ім'я пасажира чи причину скасування.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <param name="field">Назва поля для повідомлення про помилку.</param>
    /// <returns>Текст без зайвих пробілів або пояснення помилки.</returns>
    public static Result<string> ParseText(string? text, string field)
    {
        string value = Normalize(text);
        if (value.Length == 0 || value.Length > MaxTextLength)
        {
            return Result.Fail<string>($"{field}: від 1 до {MaxTextLength} символів.");
        }

        return Result.Ok(value);
    }

    /// <summary>Розбирає місце: код довідника або координати «широта; довгота».</summary>
    /// <param name="text">Введений рядок.</param>
    /// <param name="places">Довідник місць.</param>
    /// <returns>Місце або пояснення помилки.</returns>
    public static Result<Place> ParsePlace(string? text, IPlaceDirectory places)
    {
        ArgumentNullException.ThrowIfNull(places);
        string value = Normalize(text);
        Place? known = places.Find(value);
        if (known is not null)
        {
            return Result.Ok(known);
        }

        return GeoPoint.TryParse(value, out GeoPoint? point)
            ? Result.Ok(new Place(GpsCode, value, point))
            : Result.Fail<Place>(
                $"Місце «{value}» не знайдено: введіть код (P01…P50) або «50,45; 30,52».");
    }

    /// <summary>Розбирає клас автомобіля: 1, 2, 3 або назву класу.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Клас автомобіля або пояснення помилки.</returns>
    public static Result<CarClass> ParseCarClass(string? text) =>
        _carClasses.TryGetValue(Normalize(text), out CarClass carClass)
            ? Result.Ok(carClass)
            : Result.Fail<CarClass>("Клас авто: 1 — Economy, 2 — Comfort, 3 — Business.");

    /// <summary>Розбирає номер замовлення: ціле число, більше за нуль.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Номер замовлення або пояснення помилки.</returns>
    public static Result<int> ParseOrderId(string? text) =>
        int.TryParse(Normalize(text), NumberStyles.None, CultureInfo.InvariantCulture, out int id)
        && id > 0
            ? Result.Ok(id)
            : Result.Fail<int>("Номер замовлення — ціле додатне число.");

    /// <summary>
    /// Розбирає фактичний пробіг: порожній ввід означає «як довжина маршруту» (<c>null</c>).
    /// </summary>
    /// <param name="text">Введений рядок; дробова частина — через кому або крапку.</param>
    /// <returns>Пробіг, км, <c>null</c> або пояснення помилки.</returns>
    public static Result<decimal?> ParseActualKm(string? text)
    {
        string value = Normalize(text);
        if (value.Length == 0)
        {
            return Result.Ok<decimal?>(null);
        }

        bool isNumber = decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal km);
        return isNumber && km > 0 && km <= MaxActualKm
            ? Result.Ok<decimal?>(km)
            : Result.Fail<decimal?>($"Пробіг — число більше 0 і не більше {MaxActualKm} км.");
    }

    /// <summary>Розбирає дату у форматі дд.мм.рррр.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Дата або пояснення помилки.</returns>
    public static Result<DateOnly> ParseDate(string? text) =>
        DateOnly.TryParseExact(
            Normalize(text),
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly date)
            ? Result.Ok(date)
            : Result.Fail<DateOnly>("Дата у форматі дд.мм.рррр, наприклад 17.11.2026.");

    /// <summary>Розбирає необов'язкову дату для пошуку: порожній ввід — <c>null</c>.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Дата, <c>null</c> або пояснення помилки.</returns>
    public static Result<DateOnly?> ParseOptionalDate(string? text)
    {
        if (Normalize(text).Length == 0)
        {
            return Result.Ok<DateOnly?>(null);
        }

        Result<DateOnly> date = ParseDate(text);
        return date.IsSuccess
            ? Result.Ok<DateOnly?>(date.Value)
            : Result.Fail<DateOnly?>(date.Error);
    }

    /// <summary>Розбирає необов'язковий телефон для пошуку: порожній ввід — <c>null</c>.</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Телефон, <c>null</c> або пояснення помилки.</returns>
    public static Result<string?> ParseOptionalPhone(string? text)
    {
        if (Normalize(text).Length == 0)
        {
            return Result.Ok<string?>(null);
        }

        Result<string> phone = ParsePhone(text);
        return phone.IsSuccess
            ? Result.Ok<string?>(phone.Value)
            : Result.Fail<string?>(phone.Error);
    }

    /// <summary>Розбирає вид зміни: «д» або «н» (чи Day, Night).</summary>
    /// <param name="text">Введений рядок.</param>
    /// <returns>Вид зміни або пояснення помилки.</returns>
    public static Result<ShiftKind> ParseShiftKind(string? text) =>
        _shiftKinds.TryGetValue(Normalize(text), out ShiftKind kind)
            ? Result.Ok(kind)
            : Result.Fail<ShiftKind>("Зміна: «д» — денна 08:00–20:00, «н» — нічна 20:00–08:00.");

    /// <summary>Розбирає необов'язковий стан замовлення для пошуку.</summary>
    /// <param name="text">Назва стану (New, Assigned…) або порожній рядок.</param>
    /// <returns>Стан, <c>null</c> для порожнього вводу або пояснення помилки.</returns>
    public static Result<OrderStatus?> ParseOptionalStatus(string? text)
    {
        string value = Normalize(text);
        if (value.Length == 0)
        {
            return Result.Ok<OrderStatus?>(null);
        }

        return _statuses.TryGetValue(value, out OrderStatus status)
            ? Result.Ok<OrderStatus?>(status)
            : Result.Fail<OrderStatus?>(
                "Стан: New, Assigned, InProgress, Completed або Cancelled.");
    }

    private static string Normalize(string? text) => text?.Trim() ?? string.Empty;
}
