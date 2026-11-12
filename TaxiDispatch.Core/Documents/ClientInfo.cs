namespace TaxiDispatch.Core.Documents;

public sealed record ClientInfo(
    string Name,
    string Email,
    bool IsRegular);
