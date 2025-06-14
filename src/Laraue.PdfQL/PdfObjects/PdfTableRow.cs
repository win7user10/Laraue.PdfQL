using System.Text;
using Tabula;

namespace Laraue.PdfQL.PdfObjects;

public class PdfTableRow : PdfObject
{
    private readonly IReadOnlyList<Cell> _cells;

    public PdfTableRow(IReadOnlyList<Cell> cells)
    {
        _cells = cells;
    }

    public PdfTableCell? CellAt(int column)
    {
        var value = _cells.Skip(column).FirstOrDefault();
        return value is null ? null : new PdfTableCell(value);
    }

    public override string Text()
    {
        var sb = new StringBuilder();
        
        sb.Append('|');
        foreach (var cell in _cells)
        {
            sb.Append(cell.GetText());
            sb.Append('|');
        }
        
        return sb.ToString();
    }
}