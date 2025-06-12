namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record VariableExpr : Expr
{
    public VariableExpr(string name)
    {
        Name = name;
    }

    public string Name { get; init; }

    public override string ToString()
    {
        return $"${Name}";
    }
}