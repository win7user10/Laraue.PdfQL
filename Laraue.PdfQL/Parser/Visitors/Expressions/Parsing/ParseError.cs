using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;

public class ParseError
{
    public required int Position { get; init; }
    public required Token Token { get; init; }
    public required string Error { get; init; }
}