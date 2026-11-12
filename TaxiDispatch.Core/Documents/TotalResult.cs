namespace TaxiDispatch.Core.Documents;

public sealed record TotalResult(
    decimal Total,
    DocumentStatus Status);
