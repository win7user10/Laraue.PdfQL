using System.Linq.Expressions;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ExpressionCompileResult
{
    public Expression? Expression { get; set; }
    public required ExpressionCompileError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}