using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Application.Dispatch;
using TaxiDispatch.Application.Orders;
using TaxiDispatch.Application.Validation;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.App;

/// <summary>
/// Екрани сценаріїв UC-01–UC-04: питають дані через барикаду, викликають сервіс і показують
/// результат. Жодних розрахунків — лише ввід і вивід.
/// </summary>
internal sealed class OrderScreens
{
    private readonly ConsoleIo _io;
    private readonly IPlaceDirectory _places;
    private readonly OrderIntakeService _intake;
    private readonly DispatchService _dispatch;

    public OrderScreens(
        ConsoleIo io,
        IPlaceDirectory places,
        OrderIntakeService intake,
        DispatchService dispatch)
    {
        _io = io;
        _places = places;
        _intake = intake;
        _dispatch = dispatch;
    }

    public void Accept()
    {
        string phone = _io.Ask("Телефон пасажира (+380…)", InputParser.ParsePhone);
        Passenger? passenger = _intake.FindPassenger(phone);
        string? name = null;
        if (passenger is null)
        {
            _io.Say("  Новий пасажир.");
            name = _io.Ask("Ім'я пасажира", text => InputParser.ParseText(text, "Ім'я"));
        }
        else
        {
            _io.Say($"  Пасажир: {passenger.Name}");
        }

        Place pickup = AskPlace("Звідки (код P01…P50 або «широта; довгота»)");
        Place dropoff = AskPlace("Куди");
        CarClass carClass = _io.Ask(
            "Клас авто (1 — Economy, 2 — Comfort, 3 — Business)",
            InputParser.ParseCarClass);
        var request = new OrderRequest(phone, name, new TripPlan(pickup, dropoff, carClass));
        Show(_intake.Accept(request), accepted =>
            $"Прийнято замовлення № {accepted.Order.Id}: {pickup.Name} → {dropoff.Name}, "
            + $"{Formats.Km(accepted.Order.Plan.RouteKm)}, орієнтовно "
            + Formats.Money(accepted.EstimatedFare));
    }

    public void Assign()
    {
        int id = AskOrderId();
        Show(_dispatch.Assign(id), match =>
            $"Призначено водія № {match.Driver.Id} {match.Driver.Profile.Name}, "
            + $"{match.Driver.Profile.Vehicle.Model} {match.Driver.Profile.Vehicle.Plate}; "
            + $"до подачі {Formats.Km(match.PickupKm)}");
    }

    public void Start()
    {
        int id = AskOrderId();
        Show(_dispatch.Start(id), order =>
            $"Поїздка № {order.Id} почалася о {Formats.Time(order.StartedAt!.Value)}");
    }

    public void Complete()
    {
        int id = AskOrderId();
        decimal? actualKm = _io.Ask(
            "Фактичний пробіг, км (Enter — як довжина маршруту)",
            InputParser.ParseActualKm);
        Show(_dispatch.Complete(id, actualKm), result =>
            $"Поїздку № {id} завершено: {Formats.Km(result.ActualKm)}, "
            + $"до сплати {Formats.Money(result.Fare)}");
    }

    public void Cancel()
    {
        int id = AskOrderId();
        string reason = _io.Ask(
            "Причина скасування",
            text => InputParser.ParseText(text, "Причина"));
        Show(_dispatch.Cancel(id, reason), order => $"Замовлення № {order.Id} скасовано");
    }

    private int AskOrderId() => _io.Ask("Номер замовлення", InputParser.ParseOrderId);

    private Place AskPlace(string prompt) =>
        _io.Ask(prompt, text => InputParser.ParsePlace(text, _places));

    private void Show<T>(Result<T> result, Func<T, string> describe) =>
        _io.Say(result.IsSuccess ? $"  {describe(result.Value!)}" : $"  Відхилено: {result.Error}");
}
