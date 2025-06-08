using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record InstanceMethodCallExpr : Expr
{
    public InstanceMethodCallExpr(Expr o, List<Expr> arguments, Token method)
    {
        Object = o;
        Arguments = arguments;
        Method = method;
    }

    public Expr Object { get; set; }
    public List<Expr> Arguments { get; set; }
    public Token Method { get; set; }

    public override string ToString()
    {
        var arguments = string.Join(", ", Arguments.Select(a => a.ToString()));
        return $"{Object}.{Method.Lexeme}({arguments})";
    }
}