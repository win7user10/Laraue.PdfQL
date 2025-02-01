using Laraue.PQL.Stages;

namespace Laraue.PQL.Expressions;

public class PsqlApplySelectorExpression : PsqlExpression
{
    public required Type ObjectType { get; init; }
    public required Selector Selector { get; init; }
}