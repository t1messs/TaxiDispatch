namespace TaxiDispatch.Legacy;

// Об'єкт-параметр замість 11 параметрів Handle. Параметри passengerId і createdAt
// не використовувалися в тілі методу, тому не перенесені (мертвий параметр — не поведінка);
// ідентифікатор пасажира тепер є частиною Passenger.
public sealed record RideRequest(
    int RideId,
    Passenger Passenger,
    IReadOnlyList<RideLine>? Lines,
    RideState State,
    string Currency,
    bool SendMail);
