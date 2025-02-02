using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.Stages;

public class ProjectionStage : Stage
{
    public required PsqlMethodCallExpression ProjectionExpression { get; init; }
}