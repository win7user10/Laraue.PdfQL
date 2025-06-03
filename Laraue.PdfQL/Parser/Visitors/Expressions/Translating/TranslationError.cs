using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class TranslationError
{
    public Token? Token { get; set; }
    public required string Error { get; set; }
}