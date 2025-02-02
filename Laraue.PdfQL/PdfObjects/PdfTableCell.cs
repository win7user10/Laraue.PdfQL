using Tabula;

namespace Laraue.PdfQL.PdfObjects;

public class PdfTableCell : PdfObject
{
    private readonly Cell _cell;

    public PdfTableCell(Cell cell)
    {
        _cell = cell;
    }

    public string Text()
    {
        return _cell.GetText();
    }
    
    public override object ToJson()
    {
        return _cell.GetText();
    }
}