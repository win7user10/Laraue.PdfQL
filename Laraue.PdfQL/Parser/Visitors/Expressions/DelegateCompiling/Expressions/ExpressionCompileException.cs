namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling.Expressions;

public class ExpressionCompileException : Exception
{
    protected ExpressionCompileException(string message)
        : base(message)
    {}
}