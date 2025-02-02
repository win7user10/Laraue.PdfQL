using System.Linq.Expressions;
using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.TreeExecution.Expressions;

public class PSqlConstantExpressionVisitor :  PSqlExpressionVisitor<PsqlConstantExpression>
{
    public override Expression Visit(PsqlConstantExpression expression)
    {
        return Expression.Constant(expression.Value);
    }
}