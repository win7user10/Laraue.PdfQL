using Laraue.PQL.Expressions;

namespace Laraue.PQL.Stages;

public class ApplyMethodForEachElementStage : Stage
{
    public required PsqlMethodCallExpression MethodCallExpression { get; init; }
    public required Type ObjectType { get; init; }
}