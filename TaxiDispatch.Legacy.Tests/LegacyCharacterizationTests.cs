using System.Globalization;
using TaxiDispatch.Legacy;

namespace TaxiDispatch.Legacy.Tests;

// Характеризаційні тести: фіксують поточну поведінку модуля (зокрема й помилкову),
// а не те, як він мав би працювати. Очікування не змінюються під час рефакторингу.
public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // Формат чисел залежить від культури середовища, тому фіксуємо її явно.
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(null, RideState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", RideState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", RideState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", RideState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfLines, RideState state, string email, string expected)
    {
        List<RideLine>? lines = kindOfLines switch
        {
            "two" => TwoLines(),
            "empty" => new List<RideLine>(),
            _ => null,
        };
        var sut = new LegacyRideProcessor();

        string actual = sut.Handle(new RideRequest(
            1001, NewPassenger(email, PassengerKind.Regular, 0), lines, state, "UAH", false));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipRide_ReturnsExactReport()
    {
        var sut = new LegacyRideProcessor();

        string report = sut.Handle(new RideRequest(
            1001, NewPassenger("a@b.c", PassengerKind.Vip, 3), TwoLines(),
            RideState.New, "UAH", false));

        string expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "R-1 x2 = 300.00 UAH\n" +
            "R-2 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Fact]
    public void Handle_WithSendMail_WritesTwoLogEntries()
    {
        var sut = new LegacyRideProcessor();

        sut.Handle(new RideRequest(
            1001, NewPassenger("a@b.c", PassengerKind.Regular, 0), TwoLines(),
            RideState.Paid, "UAH", true));

        Assert.Equal("ok 1001\nmail -> a@b.c\n", sut.DumpLog());
    }

    [Theory]
    [InlineData(PassengerKind.Regular, 0, 760)]
    [InlineData(PassengerKind.Vip, 0, 655)]
    [InlineData(PassengerKind.Staff, 0, 550)]
    [InlineData(PassengerKind.Regular, 11, 725)]
    public void Preview_ByKind_ReturnsTotal(PassengerKind kind, int trips, decimal expected)
    {
        var sut = new LegacyRideProcessor();

        decimal total = sut.Preview(kind, trips, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_PayableExactly1000_NoShipping()
    {
        var sut = new LegacyRideProcessor();
        var lines = new List<RideLine> { new() { Code = "R-9", Km = 10, PricePerKm = 100m } };

        decimal total = sut.Preview(PassengerKind.Regular, 0, lines);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void DescribePassenger_Vip_BuildsCaption()
    {
        var passenger = new Passenger
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = PassengerKind.Vip,
            TripCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };

        // Метод залежить від системного годинника, тому очікування обчислюємо, а не вписуємо.
        int years = DateTime.Now.Year - 2020;
        string expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж " + years;

        string actual = new LegacyRideProcessor().DescribePassenger(passenger);

        Assert.Equal(expected, actual);
    }

    // Пасажир з тими самими даними, що раніше передавалися окремими параметрами Handle.
    private static Passenger NewPassenger(string email, PassengerKind kind, int trips) => new()
    {
        Id = 7,
        Name = "Іван",
        Email = email,
        Kind = kind,
        TripCount = trips,
    };

    private static List<RideLine> TwoLines() => new()
    {
        new RideLine { Code = "R-1", Km = 2, PricePerKm = 150m },
        new RideLine { Code = "R-2", Km = 1, PricePerKm = 400m },
    };
}
