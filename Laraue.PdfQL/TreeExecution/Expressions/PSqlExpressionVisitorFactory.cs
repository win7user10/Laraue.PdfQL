using System.Linq.Expressions;
using Laraue.PQL.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NotImplementedException = System.NotImplementedException;

namespace Laraue.PQL.TreeExecution.Expressions;

public class PSqlExpressionVisitorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PSqlExpressionVisitorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Expression Visit(PsqlExpression expression)
    {
        return expression switch
        {
            PsqlBinaryExpression psqlBinaryExpression => Visit(psqlBinaryExpression),
            PsqlConstantExpression psqlConstantExpression => Visit(psqlConstantExpression),
            PsqlMethodCallExpression psqlMethodCallExpression => Visit(psqlMethodCallExpression),
            PsqlParameterExpression psqlPropertyExpression => Visit(psqlPropertyExpression),
            PsqlApplySelectorExpression psqlApplySelectorExpression => Visit(psqlApplySelectorExpression),
            _ => throw new NotImplementedException()
        };
    }
    
    public Expression Visit<T>(T expression) where T : PsqlExpression
    {
        var visitor = _serviceProvider.GetService<PSqlExpressionVisitor<T>>();
        if (visitor == null)
        {
            throw new NullReferenceException($"No visitor for {typeof(T).Name}");
        }
        
        return visitor.Visit(expression);
    }
}