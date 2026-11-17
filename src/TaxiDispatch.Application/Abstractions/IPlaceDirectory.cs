using TaxiDispatch.Domain.Places;

namespace TaxiDispatch.Application.Abstractions;

/// <summary>
/// Довідник місць подачі й призначення.
/// </summary>
public interface IPlaceDirectory
{
    /// <summary>Шукає місце за кодом без урахування регістру.</summary>
    /// <param name="code">Код місця, наприклад «P07».</param>
    /// <returns>Місце або <c>null</c>, якщо коду немає.</returns>
    Place? Find(string code);

    /// <summary>Повертає всі місця довідника.</summary>
    /// <returns>Місця в порядку кодів.</returns>
    IReadOnlyList<Place> All();
}
