using System.Linq.Expressions;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.TreeExecution.Expressions.MethodCalls;

namespace Laraue.PdfQL.TreeExecution.Expressions;

public class PSqlMethodCallExpressionVisitor : PSqlExpressionVisitor<PsqlMethodCallExpression>
{
    private readonly PSqlExpressionVisitorFactory _factory;
    private readonly MethodCallVisitorFactory _methodCallVisitorFactory;

    public PSqlMethodCallExpressionVisitor(
        PSqlExpressionVisitorFactory factory,
        MethodCallVisitorFactory methodCallVisitorFactory)
    {
        _factory = factory;
        _methodCallVisitorFactory = methodCallVisitorFactory;
    }

    public override Expression Visit(PsqlMethodCallExpression expression)
    {
        var methodVisitor = _methodCallVisitorFactory.Get(expression.MethodName);
        if (methodVisitor is null)
        {
            var args = string.Join(", ", expression.MethodArguments?.Select(a => a.ToString()) ?? []);
            throw new InvalidOperationException($"Unknown method call {expression.ObjectType}.{expression.MethodName}({args})");
        }
            
        Expression[] argumentExpressions = [];
        
        var objectExpression = _factory.Visit(expression.Object);
        if (expression.MethodArguments?.Length > 0)
        {
            argumentExpressions = new Expression[expression.MethodArguments.Length];
            for (var index = 0; index < argumentExpressions.Length; index++)
            {
                argumentExpressions[index] = _factory.Visit(expression.MethodArguments[index]);
            }
        }
            
        return methodVisitor.Visit(objectExpression, argumentExpressions);
    }
}