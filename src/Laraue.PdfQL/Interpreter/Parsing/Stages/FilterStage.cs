using Laraue.PdfQL.Interpreter.Parsing.Expressions;

namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class FilterStage : Stage
{
    public FilterStage(Expr filter)
    {
        Filter = filter;
    }

    public Expr Filter { get; set; }

    public override string ToString()
    {
        return $"filter({Filter})";
    }
}