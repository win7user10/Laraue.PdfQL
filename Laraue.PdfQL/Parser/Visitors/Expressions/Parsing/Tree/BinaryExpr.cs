using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class BinaryExpr : Expr
{
    public BinaryExpr(Expr expr, TokenType operand, Expr right)
    {
        Expr = expr;
        Operand = operand;
        Right = right;
    }

    public Expr Expr { get; init; }
    public TokenType Operand { get; init; }
    public Expr Right { get; init; }
}