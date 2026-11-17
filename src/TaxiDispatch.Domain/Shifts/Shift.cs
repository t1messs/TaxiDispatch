namespace TaxiDispatch.Domain.Shifts;

/// <summary>
/// Зміна диспетчерської за датою початку (специфікація, таблиця 4.4).
/// </summary>
public sealed record Shift
{
    /// <summary>Година початку денної зміни.</summary>
    public const int DayStartHour = 8;

    /// <summary>Година початку нічної зміни.</summary>
    public const int NightStartHour = 20;

    /// <summary>
    /// Створює зміну.
    /// </summary>
    /// <param name="date">Дата початку зміни.</param>
    /// <param name="kind">Вид зміни.</param>
    public Shift(DateOnly date, ShiftKind kind)
    {
        Date = date;
        Kind = kind;
    }

    /// <summary>Отримує дату початку зміни.</summary>
    public DateOnly Date { get; }

    /// <summary>Отримує вид зміни.</summary>
    public ShiftKind Kind { get; }

    /// <summary>Отримує місцевий час початку зміни (включно).</summary>
    public DateTime Start =>
        Kind == ShiftKind.Day ? At(Date, DayStartHour) : At(Date, NightStartHour);

    /// <summary>Отримує місцевий час кінця зміни (не включно).</summary>
    public DateTime End =>
        Kind == ShiftKind.Day ? At(Date, NightStartHour) : At(Date.AddDays(1), DayStartHour);

    /// <summary>
    /// Перевіряє, чи належить момент зміні за місцевим часом. Не має побічних ефектів.
    /// </summary>
    /// <param name="moment">Момент із часовим зсувом, у якому його зафіксовано.</param>
    /// <returns><c>true</c>, якщо момент у межах [початок; кінець).</returns>
    public bool Contains(DateTimeOffset moment) =>
        moment.DateTime >= Start && moment.DateTime < End;

    private static DateTime At(DateOnly date, int hour) => date.ToDateTime(new TimeOnly(hour, 0));
}
