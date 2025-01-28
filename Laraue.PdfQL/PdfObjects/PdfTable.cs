using Tabula;

namespace Laraue.PQL.PdfObjects;

public class PdfTable : PdfObject
{
    private readonly Table _table;

    public PdfTable(Table table)
    {
        _table = table;
    }
    
    public override object ToJson()
    {
        return new
        {
            Rows = _table.Rows
                .Select(r => r
                    .Select(c => c.GetText()))
                .ToArray()
        };
    }

    public PdfCell CellAt(int index)
    {
        return new PdfCell(_table.Cells.ElementAt(index));
    }
    
    public PdfObjectContainer<PdfTableRow> GetTableRowsContainer()
    {
        return new PdfObjectContainer<PdfTableRow>(GetTableRows());
    }
    
    public PdfTableRow[] GetTableRows()
    {
        return _table.Rows.Select(r => new PdfTableRow(r)).ToArray();
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
}