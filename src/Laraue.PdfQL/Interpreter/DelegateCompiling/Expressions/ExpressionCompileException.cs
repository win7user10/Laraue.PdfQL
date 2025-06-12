namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ExpressionCompileException : Exception
{
    protected ExpressionCompileException(string message)
        : base(message)
    {}
}