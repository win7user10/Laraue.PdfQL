namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record VariableExpr : Expr
{
    public VariableExpr(string name)
    {
        Name = name;
    }

    public string Name { get; init; }
}