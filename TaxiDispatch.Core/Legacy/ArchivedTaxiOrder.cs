namespace TaxiDispatch.Core.Legacy;

// Нащадок, що звужує контракт базового типу (лістинг 4.3) — порушення LSP.
public class ArchivedTaxiOrder : LegacyTaxiOrder
{
    public ArchivedTaxiOrder(int id) => Id = id;

    public override void AddLine(LegacySegment line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
