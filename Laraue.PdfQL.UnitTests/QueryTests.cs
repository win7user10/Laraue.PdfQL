using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using Laraue.PdfQL;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Parser.Visitors;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;
using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;
using Laraue.PdfQL.Parser.Visitors.Expressions.Translating;
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
        var exp = "(item) => item.CellAt(4).Text() = \"Лейкоциты (WBC)\"";

        var scanner = new Scanner();

        var scanResult = scanner.ScanTokens(exp);

        Assert.Empty(scanResult.Errors);

        var parser = new Parser();
        
        var parseResult = parser.ParseStatement(scanResult.Tokens);
        
        Assert.Empty(parseResult.Errors);

        var translator = new Translator();

        var translationResult = translator.Translate(
            parseResult.Expression!,
            new TranslationContext { ParameterTypes = { typeof(PdfTable) }});
        
        Assert.Empty(translationResult.Errors);
    }
    
    [Fact]
    public void Scanner2Tests()
    {
        var pipeline = @"
select(tables)
	->filter((item) => item.CellAt(4).Text() = 'Лейкоциты (WBC)')
	->selectMany(tableRows)
	->map((item) => item.CellAt(1))
	->filter((item) => item.TryParse(float))";
        
        var scanner = new Scanner();

        var scanResult = scanner.ScanTokens(pipeline);

        Assert.Empty(scanResult.Errors);

        var parser = new Parser();
        
        var parseResult = parser.ParseStatement(scanResult.Tokens);
        
        Assert.Empty(parseResult.Errors);
        Assert.NotNull(parseResult.Expression);

        var translator = new Translator();

        var translationResult = translator.Translate(
            parseResult.Expression!,
            new TranslationContext { ParameterTypes = { typeof(PdfTable) }});
        
        Assert.Empty(translationResult.Errors);
    }
    
    [Fact]
    public void PSqlSyntaxTree_ShouldCreatesCorrectly_WhenStagesJsonPassed()
    {
        var stagesJson = @"
[
	{
		""$stage"": ""select"",
		""selector"": ""tables""
	},
	{
		""$stage"": ""filter"",
		""expression"": ""(item) => item.CellAt(4).Text() = 'Лейкоциты (WBC)'""
	},
	{
		""$stage"": ""selectMany"",
		""selector"": ""tableRows""
	},
	{
		""$stage"": ""map"",
		""expression"": ""(item) => item.CellAt(1)""
	},
	{
		""$stage"": ""filter"",
		""expression"": ""(item) => item.TryParse('float')""
	}
]";
        var result = PdfQLInstance.GetTreeBuilder()
            .ParseStages(stagesJson);
    }
    
    [Fact]
    public void PSqlSyntaxTree_ShouldWorkCorrectly_WhenStagesListPassed()
    {
        var pdfBytes = File.ReadAllBytes("C:\\Users\\Ilya\\Downloads\\Telegram Desktop\\analyze_2023_12_14_15_12_23.pdf");
        
        var pdfContainer = new PdfDocument(pdfBytes);
        
        Stage[] stages =
        [
            new SelectStage
            {
                SelectExpression = new PsqlApplySelectorExpression
                {
                    Selector = Selector.Tables,
                    ObjectType = typeof(PdfDocument)
                },
                ObjectType = typeof(IHasTablesContainer)
            }, // PdfObjectContainer<object> -> PdfObjectContainer<Table> // from pdf select tables as t
            new FilterStage
            {
                ObjectType = typeof(PdfTable)
            }, // PdfObjectContainer<Table>[5] -> PdfObjectContainer<Table>[1] // where t.CellAt(4).Text() = "Лейкоциты (WBC)"
            new SelectManyStage
            {
                SelectExpression = new PsqlApplySelectorExpression
                {
                    Selector = Selector.TableRows,
                    ObjectType = typeof(IHasTableRowsContainer)
                },
                ObjectType = typeof(IHasTableRowsContainer)
            }, // PdfObjectContainer<Table>[1] -> PdfObjectContainer<TableRow>[8] // from t select tableRows
            new MapStage
            {
                MethodCallExpression = new PsqlMethodCallExpression
                {
                    Object = new PsqlParameterExpression
                    {
                        ParameterName = "container",
                        Type = typeof(PdfTableRow)
                    },
                    MethodName = "CellAt",
                    MethodArguments = [new PsqlConstantExpression { Value = 1 }],
                    ObjectType = typeof(PdfTableRow)
                },
                ObjectType = typeof(PdfTableRow)
            }, // PdfObjectContainer<TableRow>[8] -> PdfObjectContainer<TableCell>[8]
            new FilterStage
            {
                ObjectType = typeof(PdfTableCell)
            }, // PdfObjectContainer<TableCell>[8] -> PdfObjectContainer<TableCell>[7]
        ];

        var executor = PdfQLInstance.GetTreeExecutor(new ExecutorOptions { HandleErrors = false });
        var sourceResult = new PdfObjectStageResult(pdfContainer);
        
        var stageResult = executor.Execute(sourceResult, new StagesList { Stages = stages });
        var result = stageResult.ToJsonObject();

        var serialized = JsonSerializer.Serialize(result);
    }
}