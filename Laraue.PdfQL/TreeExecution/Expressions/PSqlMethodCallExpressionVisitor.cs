using System.Linq.Expressions;
using System.Reflection;
using Laraue.PQL.Expressions;

namespace Laraue.PQL.TreeExecution.Expressions;

public class PSqlMethodCallExpressionVisitor : PSqlExpressionVisitor<PsqlMethodCallExpression>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public PSqlMethodCallExpressionVisitor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override Expression Visit(PsqlMethodCallExpression expression)
    {
        var candidate = expression.ObjectType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == expression.MethodName);
        
        if (candidate is null)
        {
            var args = string.Join(", ", expression.MethodArguments?.Select(a => a.ToString()) ?? []);
            throw new InvalidOperationException($"Unknown method call '{expression.MethodName}({args})'");
        }
        
        var objectExpression = _factory.Visit(expression.Object);
        if (expression.MethodArguments?.Length > 0)
        {
            var argumentExpressions = new Expression[expression.MethodArguments.Length];
            for (var index = 0; index < argumentExpressions.Length; index++)
            {
                argumentExpressions[index] = _factory.Visit(expression.MethodArguments[index]);
            }
            
            return Expression.Call(objectExpression, candidate, argumentExpressions);
        }
        
        return Expression.Call(objectExpression, candidate);
    }
}