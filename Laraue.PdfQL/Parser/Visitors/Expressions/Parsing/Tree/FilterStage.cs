namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class FilterStage : Stage
{
    public FilterStage(Expr filter)
    {
        Filter = filter;
    }

    public Expr Filter { get; set; }
}