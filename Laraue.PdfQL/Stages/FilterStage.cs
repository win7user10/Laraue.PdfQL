using Laraue.PQL.Expressions;

namespace Laraue.PQL.Stages;

public class FilterStage : Stage
{
    public required PsqlBinaryExpression BinaryExpression { get; init; }
    public required Type ObjectType { get; init; }
}