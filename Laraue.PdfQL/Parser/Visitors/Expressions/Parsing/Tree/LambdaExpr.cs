using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record LambdaExpr : Expr
{
    public LambdaExpr(List<Token> parameters, Expr body)
    {
        Parameters = parameters;
        Body = body;
    }

    public List<Token> Parameters { get; init; }
    public Expr Body { get; init; }

    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.Lexeme));
        return $"({parameters}) => {Body}";
    }
}