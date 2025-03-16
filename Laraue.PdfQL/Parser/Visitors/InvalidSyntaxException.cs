namespace Laraue.PdfQL.Parser.Visitors;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException(string message) : base(message)
    {}
    
    public InvalidSyntaxException(string message, Exception e) : base(message, e)
    {}
}