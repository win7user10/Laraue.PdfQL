using Laraue.PdfQL;
using Laraue.PdfQL.Interpreter.DelegateCompiling;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.PdfObjects.Serializing;

namespace Laraue.PQL.UnitTests;

public class QueryTests
{
    private readonly PdfqlExecutor _pdfqlExecutor;
    private readonly PdfDocument _invoiceSamplePdf;
    private readonly ISerializer _serializer;
    
    public QueryTests()
    {
        _pdfqlExecutor = new PdfqlExecutor();
        _serializer = new Serializer();
        _invoiceSamplePdf = OpenPdf("InvoiceSample.pdf");
    }
    
    [Fact]
    public void Select_Tables_ReturnsTables()
    {
        var pdfql = "select(tables)";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var tables = Assert.IsType<StageResult<PdfTable>>(result);

        Assert.Single(tables);
    }
    
    [Fact]
    public void Select_TableRows_ReturnsTableRows()
    {
        var pdfql = "select(tableRows)";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var tableRows = Assert.IsType<StageResult<PdfTableRow>>(result);

        Assert.Equal(17, tableRows.Count);
    }
    
    [Fact]
    public void Select_TableCells_ReturnsTableCells()
    {
        var pdfql = "select(tableCells)";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var tableCells = Assert.IsType<StageResult<PdfTableCell>>(result);

        Assert.Equal(25, tableCells.Count);
    }
    
    [Fact]
    public void Select_WrongIdentifier_ThrowsException()
    {
        var pdfql = "select(cows)";

        var ex = Assert.Throws<PdfqlCompileException>(() => _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf));

        var error = Assert.Single(ex.Errors);
        
        Assert.Equal("Syntax error position 3 on token 'cows'. Excepted 'tables'|'tableRows'|'tableCells'.", error.Message);
    }
    
    [Fact]
    public void SelectMany_WrongUsage_ThrowsException()
    {
        var pdfql = "selectMany(tables)";

        var ex = Assert.Throws<PdfqlCompileException>(() => _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf));

        var error = Assert.Single(ex.Errors);
        
        Assert.Equal("Compile error on stage 'SelectManyStage' : PdfDocument' is not assignable to 'StageResult`1[IHasTablesContainer]'.", error.Message);
    }
    
    [Fact]
    public void SelectMany_TableRows_ReturnsTableRows()
    {
        var pdfql = "select(tables)->selectMany(tableRows)";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var tableRows = Assert.IsType<StageResult<PdfTableRow>>(result);
        
        Assert.Equal(17, tableRows.Count);
    }
    
    [Fact]
    public void SelectMany_TableCells_ReturnsTableCells()
    {
        var pdfql = "select(tables)->selectMany(tableCells)";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var tableCells = Assert.IsType<StageResult<PdfTableCell>>(result);

        Assert.Equal(25, tableCells.Count);
    }
    
    [Fact]
    public void Map_ByText_ReturnsText()
    {
        var pdfql = "select(tables)->map((item) => item.Text())";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var content = Assert.IsType<StageResult<string>>(result);

        Assert.StartsWith("||Denny Gunawan|", content[0]);
    }
    
    [Fact]
    public void Map_Sequential_ReturnsText()
    {
        var pdfql = "select(tableRows)->map((item) => item.GetCell(2))->map((item) => item.Text())";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var firstRowColumnsTexts = Assert.IsType<StageResult<string>>(result);

        Assert.Equal("Denny Gunawan", firstRowColumnsTexts[0]);
        Assert.Equal("221 Queen St", firstRowColumnsTexts[1]);
        Assert.Equal(17, firstRowColumnsTexts.Count);
    }
    
    [Fact]
    public void Filter_ByText_ReturnsFilteredSources()
    {
        var pdfql = "select(tableCells)->filter(item => item.Text() == 'Denny Gunawan')";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var cells = Assert.IsType<StageResult<PdfTableCell>>(result);

        var cell = Assert.Single<PdfTableCell>(cells);
        
        Assert.Equal(cell.Text(), "Denny Gunawan");
    }
    
    [Theory]
    [InlineData("first")]
    [InlineData("single")]
    [InlineData("firstOrDefault")]
    public void GetOneElement_WithFilter_ReturnsFirstElement(string stage)
    {
        var pdfql = $"select(tableCells)->{stage}((item) => item.Text() == 'Denny Gunawan')";

        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var cell = Assert.IsType<PdfTableCell>(result);
        
        Assert.Equal(cell.Text(), "Denny Gunawan");
    }

    [Theory]
    [InlineData("first")]
    [InlineData("single")]
    public void GetOneElement_NoElements_Throws(string stage)
    {
        var pdfql = $"select(tableCells)->filter((item) => item.Text() == 'abcdef')->{stage}()";
        
        var ex = Assert.Throws<PdfqlRuntimeException>(() => _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf));
        
        Assert.Contains("Sequence contains no elements", ex.Message);
    }
    
    [Fact]
    public void FirstOrDefault_NoElements_ReturnsNull()
    {
        var pdfql = $"select(tableCells)->filter((item) => item.Text() == 'abcdef')->firstOrDefault()";
        
        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);
        
        Assert.Null(result);
    }
    
    [Fact]
    public void Single_MoreThanOneElement_Throws()
    {
        var pdfql = "select(tableCells)->single()";
        
        var ex = Assert.Throws<PdfqlRuntimeException>(() => _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf));
        
        Assert.Contains("Sequence contains more than one element", ex.Message);
    }
    
    [Fact]
    public void CellAt_OnTable_Success()
    {
        var pdfql = "select(tables)->map(item => item.GetCell(2, 2).Text())->first()";
        
        var result = _pdfqlExecutor.ExecutePdfql<string>(pdfql, _invoiceSamplePdf);
        
        Assert.Equal("221 Queen St", result);
    }
    
    [Fact]
    public void CellAt_OnTableRow_Success()
    {
        var pdfql = "select(tableRows)->map(item => item.GetCell(2).Text())->first()";
        
        var result = _pdfqlExecutor.ExecutePdfql<string>(pdfql, _invoiceSamplePdf);
        
        Assert.Equal("Denny Gunawan", result);
    }
    
    [Fact]
    public void Serialize_SingleTable_Success()
    {
        var pdfql = "select(tables)->first()";
        
        var result = _pdfqlExecutor.ExecutePdfql(pdfql, _invoiceSamplePdf);

        var jsonObject = _serializer.ToJsonObject(result);

        Assert.IsType<string[][]>(jsonObject);
    }
    
    [Fact]
    public void Map_ToNewObjectType_Success()
    {
        var pdfql = "select(tables)->map(table => new { Name = 'Table', Object = table })";
        
        var result = _pdfqlExecutor.ExecutePdfql<List<object>>(pdfql, _invoiceSamplePdf);
    }

    private PdfDocument OpenPdf(string name)
    {
        var bytes = File.ReadAllBytes(Path.Combine("files", name));

        return new PdfDocument(bytes);
    }
}