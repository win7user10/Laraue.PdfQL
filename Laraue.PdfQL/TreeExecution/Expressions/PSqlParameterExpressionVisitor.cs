using System.Linq.Expressions;
using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.TreeExecution.Expressions;

public class PSqlParameterExpressionVisitor : PSqlExpressionVisitor<PsqlParameterExpression>
{
    public override Expression Visit(PsqlParameterExpression expression)
    {
        return Expression.Parameter(expression.Type, expression.ParameterName);
    }
}