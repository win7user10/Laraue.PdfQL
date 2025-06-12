using Tabula;

namespace Laraue.PdfQL.PdfObjects;

public class PdfTableCell : PdfObject
{
    private readonly Cell _cell;

    public PdfTableCell(Cell cell)
    {
        _cell = cell;
    }

    public override string Text()
    {
        return _cell.GetText();
    }
}