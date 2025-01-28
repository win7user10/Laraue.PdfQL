using System.Linq.Expressions;
using Laraue.PQL.Expressions;

namespace Laraue.PQL.TreeExecution.Expressions;

public abstract class PSqlExpressionVisitor<TPsqlExpression> where TPsqlExpression : PsqlExpression
{
    public abstract Expression Visit(TPsqlExpression expression);
}