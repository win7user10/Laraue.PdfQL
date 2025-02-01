using System.Linq.Expressions;
using System.Text.Json;
using Laraue.PQL.Expressions;
using Laraue.PQL.PdfObjects;
using Laraue.PQL.PdfObjects.Interfaces;
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
                SelectExpression = new PsqlApplySelectorExpression
                {
                    Selector = Selector.Tables,
                    ObjectType = typeof(PdfDocument)
                },
                ObjectType = typeof(IHasTablesContainer)
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
                SelectExpression = new PsqlApplySelectorExpression
                {
                    Selector = Selector.TableRows,
                    ObjectType = typeof(IHasTableRowsContainer)
                },
                ObjectType = typeof(IHasTableRowsContainer)
            }, // PdfObjectContainer<Table>[1] -> PdfObjectContainer<TableRow>[8]
            new ApplyMethodForEachElementStage
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
            
            // TODO - correct way to divide apply on each element / elements container?
        ];

        var sp = new ServiceCollection()
            .AddSingleton(new ExecutorOptions { HandleErrors = false })
            .AddSingleton<ExecutorFactory>()
            .AddSingleton<PSqlExpressionVisitorFactory>()
            .AddSingleton<Executor<StagesList>, StagesListExecutor>()
            .AddSingleton<Executor<SelectStage>, SelectStageExecutor>()
            .AddSingleton<Executor<SelectManyStage>, SelectManyStageExecutor>()
            .AddSingleton<Executor<FilterStage>, FilterStageExecutor>()
            .AddSingleton<Executor<ApplyMethodForEachElementStage>, ApplyMethodStageExecutor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlBinaryExpression>, PSqlBinaryExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlMethodCallExpression>, PSqlMethodCallExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlParameterExpression>, PSqlParameterExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlConstantExpression>, PSqlConstantExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlApplySelectorExpression>, PsqlApplySelectorExpressionVisitor>()
            .BuildServiceProvider();
        
        var executor = sp.GetRequiredService<Executor<StagesList>>();
        var sourceResult = new PdfObjectStageResult(pdfContainer);
        
        var stageResult = executor.Execute(sourceResult, new StagesList { Stages = stages });
        var result = stageResult.ToJsonObject();

        var serialized = JsonSerializer.Serialize(result);
    }
}