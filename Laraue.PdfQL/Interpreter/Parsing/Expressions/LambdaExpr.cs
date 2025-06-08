using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

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