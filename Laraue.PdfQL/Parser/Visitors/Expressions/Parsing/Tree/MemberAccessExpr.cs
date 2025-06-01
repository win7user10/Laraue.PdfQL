using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

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