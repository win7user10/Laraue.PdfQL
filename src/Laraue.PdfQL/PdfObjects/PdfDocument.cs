using Laraue.PdfQL.PdfObjects.Interfaces;
using Tabula;
using UglyToad.PdfPig;

namespace Laraue.PdfQL.PdfObjects;

public class PdfDocument : PdfObject, IHasTablesContainer, IHasTableRowsContainer, IHasTableCellsContainer
{
    internal readonly UglyToad.PdfPig.PdfDocument SourceDocument;

    public PdfDocument(byte[] pdfBytes)
    {
        SourceDocument = UglyToad.PdfPig.PdfDocument.Open(pdfBytes, new ParsingOptions { ClipPaths = true });
    }

    public override string ToString()
    {
        return $"PdfDocument {SourceDocument.Information}";
    }

    public StageResult<PdfTableCell> GetTableCellsContainer()
    {
        var tableCells = GetTables()
            .SelectMany(t => t.Cells)
            .Select(c => new PdfTableCell(c))
            .ToArray();
        
        return new StageResult<PdfTableCell>(tableCells);
    }

    public StageResult<PdfTableRow> GetTableRowsContainer()
    {
        var tableRows = GetTables()
            .SelectMany(t => t.Rows)
            .Select(r => new PdfTableRow(r))
            .ToArray();

        return new StageResult<PdfTableRow>(tableRows);
    }

    public StageResult<PdfTable> GetTablesContainer()
    {
        var tables = GetTables()
            .Select(t => new PdfTable(t))
            .ToArray();

        return new StageResult<PdfTable>(tables);
    }

    private IEnumerable<Table> GetTables()
    {
        return SourceDocument.GetPages()
            .SelectMany(p =>
            {
                var oe = ObjectExtractor.ExtractPage(p);
                return Defaults.ExtractionAlgorithm.Extract(oe);
            });
    }

    public override string Text()
    {
        throw new NotImplementedException();
    }
}