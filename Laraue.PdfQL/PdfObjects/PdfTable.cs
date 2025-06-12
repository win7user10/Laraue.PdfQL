using System.Text;
using Laraue.PdfQL.PdfObjects.Interfaces;
using Tabula;

namespace Laraue.PdfQL.PdfObjects;

public class PdfTable : PdfObject, IHasTableRowsContainer, IHasTableCellsContainer
{
    private readonly Table _table;

    public PdfTable(Table table)
    {
        _table = table;
    }
    
    public StageResult<PdfTableRow> GetTableRowsContainer()
    {
        var result = _table.Rows
            .Select(r => new PdfTableRow(r))
            .ToArray();
        
        return new StageResult<PdfTableRow>(result);
    }

    public override string ToString()
    {
        var firstRowCells = _table.Rows
            .FirstOrDefault()?
            .Select(c => c.GetText());
        
        var text = firstRowCells != null
            ? string.Join(", ", firstRowCells)
            : string.Empty;
        
        return $"PdfTable {_table.ColumnCount}x{_table.RowCount} ({text})";
    }

    public override string Text()
    {
        var sb = new StringBuilder();
        foreach (var row in _table.Rows)
        {
            sb.Append('|');
            
            foreach (var cell in row)
            {
                sb.Append(cell.GetText());
                sb.Append('|');
            }
            
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }

    public StageResult<PdfTableCell> GetTableCellsContainer()
    {
        var cells = _table.Cells
            .Select(c => new PdfTableCell(c))
            .ToArray();

        return new StageResult<PdfTableCell>(cells);
    }
}