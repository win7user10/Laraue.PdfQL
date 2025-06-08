using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using Laraue.PdfQL;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Parser.Visitors;
using Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;
using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.PdfObjects.Interfaces;
using Laraue.PdfQL.StageResults;
using Laraue.PdfQL.Stages;
using Laraue.PdfQL.TreeExecution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Stage = Laraue.PdfQL.Stages.Stage;

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
        var pipeline = @"
select(tables)
	->filter((item) => item.CellAt(4).Text() = 'Лейкоциты (WBC)')
	->selectMany(tableRows)
	->map((item) => item.CellAt(1))";
        
        var scanner = new Scanner();
        var scanResult = scanner.ScanTokens(pipeline);
        Assert.Empty(scanResult.Errors);

        var parser = new Parser();
        var parseResult = parser.Parse(scanResult.Tokens);
        Assert.Empty(parseResult.Errors);
        Assert.NotEmpty(parseResult.Stages);

        var delegateCompiler = new DelegateCompiler();
        var delegateCompilingResult = delegateCompiler.Compile(parseResult.Stages.ToArray());
        Assert.Empty(delegateCompilingResult.Errors);
        Assert.NotNull(delegateCompilingResult.Delegate);
        
        var pdfBytes = File.ReadAllBytes("C:\\Users\\Ilya\\Downloads\\Telegram Desktop\\analyze_2023_12_14_15_12_23.pdf");
        var pdfContainer = new PdfDocument(pdfBytes);

        var @delegate = delegateCompilingResult.Delegate;
        var result = @delegate(pdfContainer);
    }
}