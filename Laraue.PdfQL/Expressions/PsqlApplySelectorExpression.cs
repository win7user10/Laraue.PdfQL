using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Expressions;

public class PsqlApplySelectorExpression : PsqlExpression
{
    public required Type ObjectType { get; init; }
    public required Selector Selector { get; init; }
}