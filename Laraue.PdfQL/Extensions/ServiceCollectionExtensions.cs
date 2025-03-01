using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Parser;
using Laraue.PdfQL.Parser.Visitors;
using Laraue.PdfQL.Stages;
using Laraue.PdfQL.TreeExecution;
using Laraue.PdfQL.TreeExecution.Expressions;
using Laraue.PdfQL.TreeExecution.Expressions.MethodCalls;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExecutorServices(this IServiceCollection serviceCollection, ExecutorOptions options)
    {
        return serviceCollection
            .AddSingleton(options)
            .AddSingleton<ExecutorFactory>()
            .AddSingleton<PSqlExpressionVisitorFactory>()
            .AddSingleton<Executor<StagesList>, StagesListExecutor>()
            .AddSingleton<Executor<SelectStage>, SelectStageExecutor>()
            .AddSingleton<Executor<SelectManyStage>, SelectManyStageExecutor>()
            .AddSingleton<Executor<FilterStage>, FilterStageExecutor>()
            .AddSingleton<Executor<MapStage>, ApplyMethodStageExecutor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlBinaryExpression>, PSqlBinaryExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlMethodCallExpression>, PSqlMethodCallExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlParameterExpression>, PSqlParameterExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlConstantExpression>, PSqlConstantExpressionVisitor>()
            .AddSingleton<PSqlExpressionVisitor<PsqlApplySelectorExpression>, PsqlApplySelectorExpressionVisitor>()
            .AddSingleton<MethodCallVisitorFactory>()
            .AddKeyedSingleton<MethodCallVisitor, TryParseMethodCallVisitor>("TryParse")
            .AddKeyedSingleton<MethodCallVisitor, TextMethodCallVisitor>("Text")
            .AddKeyedSingleton<MethodCallVisitor, CellAtMethodCallVisitor>("CellAt");
    }
    
    public static IServiceCollection AddParserServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<StageTokenVisitor<SelectStageToken>, SelectStageTokenVisitor>()
            .AddSingleton<PdfExpressionParser>();
    }
}