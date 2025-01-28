using Laraue.PQL.Expressions;

namespace Laraue.PQL.Stages;

public class ProjectionStage : Stage
{
    public required PsqlMethodCallExpression ProjectionExpression { get; init; }
}