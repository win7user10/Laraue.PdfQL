namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record LiteralExpr : Expr
{
    public LiteralExpr(object? value)
    {
        Value = value;
    }

    public object? Value { get; init; }

    public override string ToString()
    {
        return Value is string
            ? $"'{Value}'"
            : Value?.ToString() ?? "null";
    }
}