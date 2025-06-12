using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record UnaryExpr : Expr
{
    public UnaryExpr(Token @operator, Expr expr)
    {
        Operator = @operator;
        Expr = expr;
    }

    public Token Operator { get; init; }
    public Expr Expr { get; init; }
}