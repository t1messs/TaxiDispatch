namespace TaxiDispatch.Tests.Doubles;

// Тестовий дублер годинника: час задає тест, а не система. Часовий пояс — UTC+2,
// тож «місцевий» час у тестах однаковий на будь-якій машині й у конвеєрі.
internal sealed class FixedTimeProvider : TimeProvider
{
    private static readonly TimeZoneInfo _zone = TimeZoneInfo.CreateCustomTimeZone(
        "UTC+02",
        TimeSpan.FromHours(2),
        "UTC+02",
        "UTC+02");

    private DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now) => _now = now;

    public override TimeZoneInfo LocalTimeZone => _zone;

    public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

    public void Set(DateTimeOffset now) => _now = now;
}
