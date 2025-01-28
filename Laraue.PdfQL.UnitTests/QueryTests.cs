using System.Text.Json;
using Laraue.PQL.Expressions;
using Laraue.PQL.PdfObjects;
using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;
using Laraue.PQL.TreeExecution;
using Laraue.PQL.TreeExecution.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PQL.UnitTests;

public class QueryTests
{
    [Fact]
    public void PSqlSyntaxTree_ShouldWorkCorrectly_WhenStagesListPassed()
    {
        var pdfBytes = File.ReadAllBytes("C:\\Users\\Ilya\\Downloads\\Telegram Desktop\\analyze_2023_12_14_15_12_23.pdf");
        
        var pdfContainer = new PdfDocument(pdfBytes);
        
        Stage[] stages =
        [
            new SelectStage
            {
                Selector = Selector.Tables
            }, // PdfObjectContainer<object> -> PdfObjectContainer<Table>
            new FilterStage
            {
                BinaryExpression = new PsqlBinaryExpression
                {
                    Left = new PsqlMethodCallExpression
                    {
                        MethodName = "Text",
                        Object = new PsqlMethodCallExpression
                        {
                            Object = new PsqlParameterExpression
                            {
                                ParameterName = "tables",
                                Type = typeof(PdfTable)
                            },
                            MethodName = "CellAt",
                            MethodArguments = [new PsqlConstantExpression { Value = 4 }],
                            ObjectType = typeof(PdfTable)
                        },
                        ObjectType = typeof(PdfCell)
                    },
                    Right = new PsqlConstantExpression { Value = "Лейкоциты (WBC)" },
                    Operator = PsqlOperand.Equal
                },
                ObjectType = typeof(PdfTable)
            }, // PdfObjectContainer<Table>[5] -> PdfObjectContainer<Table>[1]
            new SelectManyStage
            {
                Selector = Selector.TableRows
            }, // PdfObjectContainer<Table>[1] -> PdfObjectContainer<TableRow>[8]
        ];

        var sp = new ServiceCollection()
            .AddSingleton<ExecutorFactory>()
            .AddSingleton<PSqlExpressionVisitorFactory>()
            .AddSingleton<Executor<StagesList>, StagesListExecutor>()
            .AddSingleton<Executor<SelectStage>, SelectStageExecutor>()
            .AddSingleton<Executor<SelectManyStage>, SelectManyStageExecutor>()
            .AddSingleton<Executor<FilterStage>, FilterStageExecutor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlBinaryExpression>, PSqlBinaryExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlMethodCallExpression>, PSqlMethodCallExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlParameterExpression>, PSqlParameterExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlConstantExpression>, PSqlConstantExpressionVisitor>()
            .BuildServiceProvider();
        
        var executor = sp.GetRequiredService<Executor<StagesList>>();
        var sourceResult = new PdfObjectStageResult(pdfContainer);
        
        var stageResult = executor.Execute(sourceResult, new StagesList { Stages = stages });
        var result = stageResult.ToJsonObject();

        var serialized = JsonSerializer.Serialize(result);
    }
}