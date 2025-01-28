using System.Linq.Expressions;
using Laraue.PQL.Expressions;

namespace Laraue.PQL.TreeExecution.Expressions;

public class PSqlParameterExpressionVisitor : PSqlExpressionVisitor<PsqlParameterExpression>
{
    public override Expression Visit(PsqlParameterExpression expression)
    {
        return Expression.Parameter(expression.Type, expression.ParameterName);
    }
}