using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.Tests.Doubles;

// Тестовий дублер сховища: дані в пам'яті замість файлів JSON. SaveCount показує,
// скільки разів сценарій зберіг зміни, — так тест бачить, що збереження було (чи не було).
internal sealed class FakeStore : ITaxiStore
{
    private readonly FakeOrders _orders = new();
    private readonly FakeDrivers _drivers = new();
    private readonly FakePassengers _passengers = new();
    private readonly FakePlaces _places = new();

    public IOrderRepository Orders => _orders;

    public IDriverRepository Drivers => _drivers;

    public IPassengerRepository Passengers => _passengers;

    public IPlaceDirectory Places => _places;

    public int SaveCount { get; private set; }

    public void SaveChanges() => SaveCount++;

    public FakeStore With(params Driver[] drivers)
    {
        _drivers.Items.AddRange(drivers);
        return this;
    }

    public FakeStore With(params TaxiOrder[] orders)
    {
        _orders.Items.AddRange(orders);
        return this;
    }

    public FakeStore With(params Passenger[] passengers)
    {
        _passengers.Items.AddRange(passengers);
        return this;
    }

    public FakeStore With(params Place[] places)
    {
        _places.Items.AddRange(places);
        return this;
    }

    private sealed class FakeOrders : IOrderRepository
    {
        public List<TaxiOrder> Items { get; } = [];

        public TaxiOrder? Find(int id) => Items.Find(order => order.Id == id);

        public IReadOnlyList<TaxiOrder> All() => Items;

        public int NextId() => Items.Count == 0 ? 1 : Items.Max(order => order.Id) + 1;

        public void Add(TaxiOrder order) => Items.Add(order);
    }

    private sealed class FakeDrivers : IDriverRepository
    {
        public List<Driver> Items { get; } = [];

        public Driver? Find(int id) => Items.Find(driver => driver.Id == id);

        public IReadOnlyList<Driver> All() => Items;
    }

    private sealed class FakePassengers : IPassengerRepository
    {
        public List<Passenger> Items { get; } = [];

        public Passenger? FindByPhone(string phone) =>
            Items.Find(passenger => passenger.Phone == phone);

        public int NextId() => Items.Count + 1;

        public void Add(Passenger passenger) => Items.Add(passenger);
    }

    private sealed class FakePlaces : IPlaceDirectory
    {
        public List<Place> Items { get; } = [];

        public Place? Find(string code) => Items.Find(place =>
            string.Equals(place.Code, code, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<Place> All() => Items;
    }
}
