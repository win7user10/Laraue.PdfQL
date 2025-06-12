namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class DelegateRuntimeException : Exception
{
    public DelegateRuntimeException(string message) : base(message)
    {}
}