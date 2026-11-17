namespace TaxiDispatch.Core.Documents;

/// <summary>
/// Дані пасажира в документі розрахунку (назва порушує N-18, борг TD-09).
/// </summary>
/// <param name="Name">Ім'я пасажира.</param>
/// <param name="Email">Електронна пошта пасажира.</param>
/// <param name="IsRegular">Ознака постійного пасажира.</param>
public sealed record ClientInfo(
    string Name,
    string Email,
    bool IsRegular);
