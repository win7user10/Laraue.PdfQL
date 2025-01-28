namespace Laraue.PQL.Expressions;

public class PsqlConstantExpression : PsqlExpression
{
    public required object Value { get; set; }
}