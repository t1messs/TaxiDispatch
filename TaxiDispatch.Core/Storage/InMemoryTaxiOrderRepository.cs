using TaxiDispatch.Core.Abstractions;
using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Storage;

/// <summary>
/// Сховище замовлень у пам'яті на основі словника.
/// </summary>
public sealed class InMemoryTaxiOrderRepository : ITaxiOrderRepository
{
    private readonly Dictionary<int, TaxiOrder> _orders = new();

    /// <inheritdoc/>
    public void Add(TaxiOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException("Замовлення з таким ідентифікатором уже існує.");
        }
    }

    /// <inheritdoc/>
    public TaxiOrder? GetById(int id) => _orders.GetValueOrDefault(id);

    /// <inheritdoc/>
    public IReadOnlyList<TaxiOrder> GetAll() => _orders.Values.ToList();
}
