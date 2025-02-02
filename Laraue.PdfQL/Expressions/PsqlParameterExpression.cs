namespace Laraue.PdfQL.Expressions;

public class PsqlParameterExpression : PsqlExpression
{
    public required string ParameterName { get; init; }
    public required Type Type { get; init; }
}