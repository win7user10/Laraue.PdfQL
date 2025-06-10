using Laraue.PdfQL;
using Laraue.PdfQL.PdfObjects;

namespace Laraue.PQL.UnitTests;

public class QueryTests
{
    private readonly PSqlExecutor _pSqlExecutor;
    private readonly PdfDocument _invoiceSamplePdf;
    
    public QueryTests()
    {
        _pSqlExecutor = new PSqlExecutor();
        _invoiceSamplePdf = OpenPdf("InvoiceSample.pdf");
    }
    
    [Fact]
    public void Select_Tables_ReturnsTables()
    {
        var psql = "select(tables)";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var tables = Assert.IsType<StageResult<PdfTable>>(result);

        Assert.Single(tables);
    }
    
    [Fact]
    public void Select_TableRows_ReturnsTableRows()
    {
        var psql = "select(tableRows)";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var tableRows = Assert.IsType<StageResult<PdfTableRow>>(result);

        Assert.Equal(17, tableRows.Count);
    }
    
    [Fact]
    public void Select_TableCells_ReturnsTableCells()
    {
        var psql = "select(tableCells)";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var tableCells = Assert.IsType<StageResult<PdfTableCell>>(result);

        Assert.Equal(25, tableCells.Count);
    }

    private PdfDocument OpenPdf(string name)
    {
        var bytes = File.ReadAllBytes(Path.Combine("files", name));

        return new PdfDocument(bytes);
    }
}