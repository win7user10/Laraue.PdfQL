using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;

public interface IParser
{
    ParseResult Parse(Token[] tokens);
}