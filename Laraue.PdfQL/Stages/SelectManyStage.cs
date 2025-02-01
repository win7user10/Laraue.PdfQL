using Laraue.PQL.Expressions;

namespace Laraue.PQL.Stages;

public class SelectManyStage : Stage
{
    public required PsqlExpression SelectExpression { get; init; }
    public required Type ObjectType { get; init; }
}