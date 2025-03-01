using Laraue.PdfQL.Expressions;

namespace Laraue.PdfQL.Stages;

public class MapStage : Stage
{
    public required PsqlMethodCallExpression MethodCallExpression { get; init; }
    public required Type ObjectType { get; init; }
}