using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record MethodCallExpr : Expr
{
    public MethodCallExpr(Expr callee, Token paren, List<Expr> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
    }

    public Expr Callee { get; init; }
    public Token Paren { get; init; }
    public List<Expr> Arguments { get; init; }
}