using System.Diagnostics;
using Laraue.PdfQL;
using Laraue.PdfQL.Interpreter.DelegateCompiling;
using Laraue.PdfQL.Interpreter.Parsing;
using Laraue.PdfQL.Interpreter.Scanning;
using Laraue.PdfQL.PdfObjects;
using Xunit.Abstractions;

namespace Laraue.PQL.UnitTests;

public class QueryTests
{
    public QueryTests(ITestOutputHelper testOutputHelper)
    {
        Trace.Listeners.Add(new TraceListener1(testOutputHelper));
    }

    public class TraceListener1 : TraceListener
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TraceListener1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        public override void Write(string? message)
        {
            _testOutputHelper.WriteLine(message);
        }

        public override void WriteLine(string? message)
        {
            _testOutputHelper.WriteLine(message);
        }
    }
    
    [Fact]
    public void ScannerTests()
    {
        var psql = @"
select(tables)
	->filter((item) => item.CellAt(4).Text() = 'Лейкоциты (WBC)')
	->selectMany(tableRows)
	->map((item) => item.CellAt(1))";

        var pdfBytes = File.ReadAllBytes("C:\\Users\\Ilya\\Downloads\\Telegram Desktop\\analyze_2023_12_14_15_12_23.pdf");
        var pdfContainer = new PdfDocument(pdfBytes);
        
        var executor = new PSqlExecutor();
        var result = executor.ExecutePsql(psql, pdfContainer);

        Assert.NotNull(result);
    }
}