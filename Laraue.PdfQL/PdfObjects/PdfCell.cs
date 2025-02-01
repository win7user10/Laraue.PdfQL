using Tabula;

namespace Laraue.PQL.PdfObjects;

public class PdfCell : PdfObject
{
    private readonly Cell _cell;

    public PdfCell(Cell cell)
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