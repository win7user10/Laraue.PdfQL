using System.Text;
using Laraue.PdfQL.PdfObjects.Interfaces;
using Tabula;

namespace Laraue.PdfQL.PdfObjects;

public class PdfTableRow : PdfObject, IHasTableCellsContainer
{
    private readonly IReadOnlyList<Cell> _cells;

    public PdfTableRow(IReadOnlyList<Cell> cells)
    {
        _cells = cells;
    }

    public PdfTableCell? GetCell(int column)
    {
        var value = _cells.GetByCellNumber(column);
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

    public StageResult<PdfTableCell> GetTableCellsContainer()
    {
        var cells = _cells
            .Select(c => new PdfTableCell(c))
            .ToArray();

        return new StageResult<PdfTableCell>(cells);
    }
}