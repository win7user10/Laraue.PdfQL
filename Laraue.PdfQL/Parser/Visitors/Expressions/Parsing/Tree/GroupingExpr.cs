namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record GroupingExpr : Expr
{
    public GroupingExpr(Expr expr)
    {
        Expr = expr;
    }

    public Expr Expr { get; init; }

    public override string ToString()
    {
        return $"({Expr})";
    }
}