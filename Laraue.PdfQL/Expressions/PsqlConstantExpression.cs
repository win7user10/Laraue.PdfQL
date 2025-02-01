namespace Laraue.PQL.Expressions;

public class PsqlConstantExpression : PsqlExpression
{
    public required object Value { get; set; }

    public override string ToString()
    {
        return Value.ToString()!;
    }
}