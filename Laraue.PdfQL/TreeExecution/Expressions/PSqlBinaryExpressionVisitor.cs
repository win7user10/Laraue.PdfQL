using System.Linq.Expressions;
using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.TreeExecution.Expressions;

public class PSqlBinaryExpressionVisitor : PSqlExpressionVisitor<PsqlBinaryExpression>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public PSqlBinaryExpressionVisitor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override Expression Visit(PsqlBinaryExpression expression)
    {
        var leftExpression = _factory.Visit(expression.Left);
        var rightExpression = _factory.Visit(expression.Right);
        var expressionType = GetOperand(expression.Operator);

        return Expression.MakeBinary(expressionType, leftExpression, rightExpression);
    }

    private ExpressionType GetOperand(PsqlOperand operand)
    {
        return operand switch
        {
            PsqlOperand.Equal => ExpressionType.Equal,
            _ => throw new NotImplementedException()
        };
    }
}