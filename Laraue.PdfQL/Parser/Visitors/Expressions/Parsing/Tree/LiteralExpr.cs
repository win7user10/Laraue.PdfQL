namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class LiteralExpr : Expr
{
    public LiteralExpr(object? value)
    {
        Value = value;
    }

    public object? Value { get; init; }
}