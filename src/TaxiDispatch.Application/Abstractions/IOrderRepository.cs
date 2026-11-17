using TaxiDispatch.Domain.Orders;

namespace TaxiDispatch.Application.Abstractions;

/// <summary>
/// Сховище замовлень: пошук, перелік і додавання; збереження — через <see cref="ITaxiStore"/>.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Шукає замовлення за номером.</summary>
    /// <param name="id">Номер замовлення.</param>
    /// <returns>Замовлення або <c>null</c>, якщо його немає.</returns>
    TaxiOrder? Find(int id);

    /// <summary>Повертає всі замовлення.</summary>
    /// <returns>Замовлення в порядку додавання.</returns>
    IReadOnlyList<TaxiOrder> All();

    /// <summary>Видає номер для нового замовлення: найбільший наявний плюс один.</summary>
    /// <returns>Вільний номер замовлення.</returns>
    int NextId();

    /// <summary>Додає нове замовлення.</summary>
    /// <param name="order">Замовлення з унікальним номером.</param>
    void Add(TaxiOrder order);
}
