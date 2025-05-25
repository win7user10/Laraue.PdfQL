using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;

public interface IParser
{
    ParseResult ParseStatement(Token[] tokens);
    ParseResult ParseEquality(Token[] tokens);
}