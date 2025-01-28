using Tabula;

namespace Laraue.PQL.PdfObjects;

public class PdfTableRow : PdfObject
{
    private readonly IReadOnlyList<Cell> _cells;

    public PdfTableRow(IReadOnlyList<Cell> cells)
    {
        _cells = cells;
    }

    public override object ToJson()
    {
        return _cells.Select(c => c.GetText()).ToArray();
    }
}