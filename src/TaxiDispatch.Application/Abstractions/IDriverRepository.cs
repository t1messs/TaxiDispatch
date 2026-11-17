using TaxiDispatch.Domain.Fleet;

namespace TaxiDispatch.Application.Abstractions;

/// <summary>
/// Довідник водіїв автопарку.
/// </summary>
public interface IDriverRepository
{
    /// <summary>Шукає водія за номером.</summary>
    /// <param name="id">Номер водія.</param>
    /// <returns>Водій або <c>null</c>, якщо його немає.</returns>
    Driver? Find(int id);

    /// <summary>Повертає всіх водіїв.</summary>
    /// <returns>Водії в порядку номерів.</returns>
    IReadOnlyList<Driver> All();
}
