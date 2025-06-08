namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public class DelegateRuntimeException : Exception
{
    public DelegateRuntimeException(string message) : base(message)
    {}
}