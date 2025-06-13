using Laraue.PdfQL.Interpreter.Parsing.Expressions;

namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class FirstStage : OneElementStage
{
    public const string Name = "first";
    
    public FirstStage(Expr? filter) : base(filter)
    {
        Filter = filter;
    }

    public Expr? Filter { get; set; }

    public override string ToString()
    {
        return $"{Name}({Filter})";
    }
}