using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record BinaryExpr : Expr
{
    public BinaryExpr(Expr left, Token @operator, Expr right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public Expr Left { get; init; }
    public Token Operator { get; init; }
    public Expr Right { get; init; }
}