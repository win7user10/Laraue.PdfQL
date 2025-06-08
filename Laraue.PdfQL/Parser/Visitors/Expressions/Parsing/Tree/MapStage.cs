namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class MapStage : Stage
{
    public MapStage(Expr projection)
    {
        Projection = projection;
    }

    public Expr Projection { get; set; }
}