namespace Laraue.PQL.Expressions;

public class PsqlBinaryExpression : PsqlExpression
{
    public required PsqlExpression Left { get; init; }
    public required PsqlExpression Right { get; init; }
    public required PsqlOperand Operator { get; init; }
}