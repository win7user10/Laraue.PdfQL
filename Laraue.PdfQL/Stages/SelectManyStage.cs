using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.Stages;

public class SelectManyStage : Stage
{
    public required PsqlExpression SelectExpression { get; init; }
    public required Type ObjectType { get; init; }
}