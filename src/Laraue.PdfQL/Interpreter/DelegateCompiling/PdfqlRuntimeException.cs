namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class PdfqlRuntimeException : Exception
{
    public PdfqlRuntimeException(string message) : base(message)
    {}
}