namespace Laraue.PdfQL.TreeExecution.Expressions.Exceptions;

public class UnknownMethodCallException : Exception
{
    public UnknownMethodCallException(string message) : base(message)
    {}
}