namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class PSqlRuntimeException : Exception
{
    public PSqlRuntimeException(string message) : base(message)
    {}
}