namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class TranslationException : Exception
{
    protected TranslationException(string message)
        : base(message)
    {}
}