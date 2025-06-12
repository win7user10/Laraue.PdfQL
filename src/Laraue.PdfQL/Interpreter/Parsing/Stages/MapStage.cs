using Laraue.PdfQL.Interpreter.Parsing.Expressions;

namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class MapStage : Stage
{
    public MapStage(Expr projection)
    {
        Projection = projection;
    }

    public Expr Projection { get; set; }
}