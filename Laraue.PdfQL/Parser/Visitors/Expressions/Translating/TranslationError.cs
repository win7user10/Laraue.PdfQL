namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class TranslationError
{
    public int Position { get; set; }
    public required string Error { get; set; }
}