using Laraue.PdfQL.Interpreter.Parsing.Expressions;

namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class OneElementStage : Stage
{
    public OneElementStage(Expr? filter)
    {
        Filter = filter;
    }

    public Expr? Filter { get; set; }
}