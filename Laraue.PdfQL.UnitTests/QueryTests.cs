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
    
    [Fact]
    public void Select_WrongIdentifier_ThrowsException()
    {
        var psql = "select(cows)";

        var ex = Assert.Throws<PSqlExecutionException>(() => _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf));

        var error = Assert.Single(ex.Errors);
        
        Assert.Equal("Syntax error position 3 on token 'cows'. Excepted 'tables'|'tableRows'|'tableCells'.", error.Message);
    }
    
    [Fact]
    public void SelectMany_WrongUsage_ThrowsException()
    {
        var psql = "selectMany(tables)";

        var ex = Assert.Throws<PSqlExecutionException>(() => _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf));

        var error = Assert.Single(ex.Errors);
        
        Assert.Equal("Compile error on stage 'SelectManyStage' : PdfDocument' is not assignable to 'StageResult`1[IHasTablesContainer]'.", error.Message);
    }
    
    [Fact]
    public void SelectMany_TableRows_ReturnsTableRows()
    {
        var psql = "select(tables)->selectMany(tableRows)";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var tableRows = Assert.IsType<StageResult<PdfTableRow>>(result);
        
        Assert.Equal(17, tableRows.Count);
    }
    
    [Fact]
    public void SelectMany_TableCells_ReturnsTableCells()
    {
        var psql = "select(tables)->selectMany(tableCells)";

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