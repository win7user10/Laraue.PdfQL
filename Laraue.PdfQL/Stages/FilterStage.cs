using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.Stages;

public class FilterStage : Stage
{
    public required PsqlBinaryExpression BinaryExpression { get; init; }
    public required Type ObjectType { get; init; }
}