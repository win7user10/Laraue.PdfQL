using Tabula;

namespace Laraue.PdfQL.PdfObjects;

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

    public PdfTableCell CellAt(int index)
    {
        return new PdfTableCell(_cells.ElementAt(index));
    }
}