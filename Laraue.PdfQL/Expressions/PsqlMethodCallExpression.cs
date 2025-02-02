namespace Laraue.PdfQL.Expressions;

public class PsqlMethodCallExpression : PsqlExpression
{
    public required string MethodName { get; init; }
    public PsqlExpression[]? MethodArguments { get; init; }
    public required PsqlExpression Object { get; init; }
    public required Type ObjectType { get; init; }
}