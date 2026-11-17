using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Tests;

// Фейк: справжня, але спрощена реалізація сховища в пам'яті.
// Має власну поведінку (зберігає й знаходить) і рахує виклики Add.
internal sealed class FakeTaxiOrderRepository : ITaxiOrderRepository
{
    private readonly Dictionary<int, TaxiOrder> _orders = [];

    public int AddCalls { get; private set; }

    public void Add(TaxiOrder order)
    {
        AddCalls++;
        _orders[order.Id] = order;
    }

    public TaxiOrder? GetById(int id) => _orders.GetValueOrDefault(id);

    public IReadOnlyList<TaxiOrder> GetAll() => [.. _orders.Values];
}

// Стаб: не має логіки, лише повертає заздалегідь задану відповідь.
internal sealed class StubPricingPolicy(decimal price) : IPricingPolicy
{
    public decimal PriceOf(RouteSegment segment) => price;
}

// Пустушка (dummy): потрібна лише щоб задовольнити конструктор сервісу.
internal sealed class DummyNotifier : INotifier
{
    public void Notify(string recipient, string message)
    {
    }
}
