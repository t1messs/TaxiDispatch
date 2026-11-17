using TaxiDispatch.Application.Abstractions;
using TaxiDispatch.Domain.Fleet;
using TaxiDispatch.Domain.Orders;
using TaxiDispatch.Domain.Passengers;
using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.Infrastructure.Json;

/// <summary>
/// Дані диспетчерської у файлах JSON: читаються цілком під час запуску, змінені файли
/// (замовлення, водії, пасажири) записуються цілком після кожного сценарію.
/// </summary>
public sealed class JsonTaxiStore : ITaxiStore
{
    private const string OrdersFile = "orders.json";
    private const string DriversFile = "drivers.json";
    private const string PassengersFile = "passengers.json";
    private const string PlacesFile = "places.json";

    private readonly DataPaths _paths;
    private readonly OrderList _orders;
    private readonly DriverList _drivers;
    private readonly PassengerList _passengers;
    private readonly PlaceList _places;

    /// <summary>
    /// Завантажує всі дані. Записи перевіряються конструкторами домену — це барикада для
    /// даних із файлу: недопустимий запис зупиняє запуск із назвою файлу.
    /// </summary>
    /// <param name="paths">Розташування даних.</param>
    /// <exception cref="InvalidDataException">Файл пошкоджено або дані недопустимі.</exception>
    /// <exception cref="IOException">Файл не вдалося прочитати.</exception>
    public JsonTaxiStore(DataPaths paths)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _places = new PlaceList(Read<Place>(PlacesFile));
        _drivers = new DriverList(Read<Driver>(DriversFile));
        _passengers = new PassengerList(Read<Passenger>(PassengersFile));
        _orders = new OrderList(JsonFiles.Read<List<OrderSnapshot>, List<TaxiOrder>>(
            paths.ForReading(OrdersFile),
            snapshots => snapshots.Select(TaxiOrder.Restore).ToList()));
    }

    /// <inheritdoc/>
    public IOrderRepository Orders => _orders;

    /// <inheritdoc/>
    public IDriverRepository Drivers => _drivers;

    /// <inheritdoc/>
    public IPassengerRepository Passengers => _passengers;

    /// <inheritdoc/>
    public IPlaceDirectory Places => _places;

    /// <inheritdoc/>
    public void SaveChanges()
    {
        Directory.CreateDirectory(_paths.StateDirectory);
        List<OrderSnapshot> snapshots = _orders.Items.Select(order => order.ToSnapshot()).ToList();
        JsonFiles.Write(_paths.ForWriting(OrdersFile), snapshots);
        JsonFiles.Write(_paths.ForWriting(DriversFile), _drivers.Items);
        JsonFiles.Write(_paths.ForWriting(PassengersFile), _passengers.Items);
    }

    private List<T> Read<T>(string fileName) =>
        JsonFiles.Read<List<T>, List<T>>(_paths.ForReading(fileName), items => items);

    private sealed class OrderList : IOrderRepository
    {
        public OrderList(List<TaxiOrder> items) => Items = items;

        public List<TaxiOrder> Items { get; }

        public TaxiOrder? Find(int id) => Items.Find(order => order.Id == id);

        public IReadOnlyList<TaxiOrder> All() => Items;

        public int NextId() => Items.Count == 0 ? 1 : Items.Max(order => order.Id) + 1;

        public void Add(TaxiOrder order) => Items.Add(order);
    }

    private sealed class DriverList : IDriverRepository
    {
        public DriverList(List<Driver> items) => Items = items;

        public List<Driver> Items { get; }

        public Driver? Find(int id) => Items.Find(driver => driver.Id == id);

        public IReadOnlyList<Driver> All() => Items;
    }

    private sealed class PassengerList : IPassengerRepository
    {
        public PassengerList(List<Passenger> items) => Items = items;

        public List<Passenger> Items { get; }

        public Passenger? FindByPhone(string phone) =>
            Items.Find(passenger => passenger.Phone == phone);

        public int NextId() => Items.Count == 0 ? 1 : Items.Max(passenger => passenger.Id) + 1;

        public void Add(Passenger passenger) => Items.Add(passenger);
    }

    private sealed class PlaceList : IPlaceDirectory
    {
        public PlaceList(List<Place> items) => Items = items;

        public List<Place> Items { get; }

        public Place? Find(string code) => Items.Find(place =>
            string.Equals(place.Code, code, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<Place> All() => Items;
    }
}
