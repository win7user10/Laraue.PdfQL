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
    
    [Fact]
    public void Map_ByText_ReturnsText()
    {
        var psql = "select(tables)->map((item) => item.Text())";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var content = Assert.IsType<StageResult<string>>(result);

        Assert.StartsWith("||Denny Gunawan|", content[0]);
    }
    
    [Fact]
    public void Map_Sequential_ReturnsText()
    {
        var psql = "select(tableRows)->map((item) => item.CellAt(1))->map((item) => item.Text())";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var firstRowColumnsTexts = Assert.IsType<StageResult<string>>(result);

        Assert.Equal("Denny Gunawan", firstRowColumnsTexts[0]);
        Assert.Equal("221 Queen St", firstRowColumnsTexts[1]);
        Assert.Equal(17, firstRowColumnsTexts.Count);
    }
    
    [Fact]
    public void Filter_ByText_ReturnsFilteredSources()
    {
        var psql = "select(tableCells)->filter((item) => item.Text() = 'Denny Gunawan')";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var cells = Assert.IsType<StageResult<PdfTableCell>>(result);

        var cell = Assert.Single<PdfTableCell>(cells);
        
        Assert.Equal(cell.Text(), "Denny Gunawan");
    }
    
    [Theory]
    [InlineData("first")]
    [InlineData("single")]
    [InlineData("firstOrDefault")]
    public void GetSingleElement_WithFilter_ReturnsFirstElement(string stage)
    {
        var psql = $"select(tableCells)->{stage}((item) => item.Text() = 'Denny Gunawan')";

        var result = _pSqlExecutor.ExecutePsql(psql, _invoiceSamplePdf);

        var cell = Assert.IsType<PdfTableCell>(result);
        
        Assert.Equal(cell.Text(), "Denny Gunawan");
    }

    private PdfDocument OpenPdf(string name)
    {
        var bytes = File.ReadAllBytes(Path.Combine("files", name));

        return new PdfDocument(bytes);
    }
}