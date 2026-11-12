using TaxiDispatch.Core.Domain;

namespace TaxiDispatch.Core.Abstractions;

/// <summary>
/// Сховище замовлень таксі: відповідає лише за збереження й пошук.
/// </summary>
public interface ITaxiOrderRepository
{
    /// <summary>Додає замовлення до сховища.</summary>
    /// <param name="order">Замовлення, яке зберігається.</param>
    void Add(TaxiOrder order);

    /// <summary>Шукає замовлення за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <returns>Знайдене замовлення або null.</returns>
    TaxiOrder? GetById(int id);

    /// <summary>Повертає всі збережені замовлення.</summary>
    /// <returns>Список замовлень лише для читання.</returns>
    IReadOnlyList<TaxiOrder> GetAll();
}
