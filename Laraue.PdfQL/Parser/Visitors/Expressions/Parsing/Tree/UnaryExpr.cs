using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class UnaryExpr : Expr
{
    public UnaryExpr(TokenType operand, Expr expr)
    {
        Operand = operand;
        Expr = expr;
    }

    public TokenType Operand { get; init; }
    public Expr Expr { get; init; }
}