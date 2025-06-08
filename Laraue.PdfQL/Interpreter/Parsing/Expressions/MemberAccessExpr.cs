using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record MemberAccessExpr : Expr
{
    public MemberAccessExpr(Expr expr, Token name)
    {
        Expr = expr;
        Name = name;
    }

    public Expr Expr { get; init; }
    public Token Name { get; init; }
}