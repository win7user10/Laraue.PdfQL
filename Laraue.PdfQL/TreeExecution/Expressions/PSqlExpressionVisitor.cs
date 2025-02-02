using System.Linq.Expressions;
using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.TreeExecution.Expressions;

public abstract class PSqlExpressionVisitor<TPsqlExpression> where TPsqlExpression : PsqlExpression
{
    public abstract Expression Visit(TPsqlExpression expression);
}