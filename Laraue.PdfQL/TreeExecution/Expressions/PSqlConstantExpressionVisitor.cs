using System.Linq.Expressions;
using Laraue.PQL.Expressions;

namespace Laraue.PQL.TreeExecution.Expressions;

public class PSqlConstantExpressionVisitor :  PSqlExpressionVisitor<PsqlConstantExpression>
{
    public override Expression Visit(PsqlConstantExpression expression)
    {
        return Expression.Constant(expression.Value);
    }
}