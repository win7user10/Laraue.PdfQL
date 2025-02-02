using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.Stages;

public class SelectStage : Stage
{
    public required PsqlExpression SelectExpression { get; init; }
    public required Type ObjectType { get; init; }
}