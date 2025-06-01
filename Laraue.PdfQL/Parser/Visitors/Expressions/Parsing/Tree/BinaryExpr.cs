using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record BinaryExpr : Expr
{
    public BinaryExpr(Expr expr, Token @operator, Expr right)
    {
        Expr = expr;
        Operator = @operator;
        Right = right;
    }

    public Expr Expr { get; init; }
    public Token Operator { get; init; }
    public Expr Right { get; init; }
}